using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CbgDesktopClient
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;

        private System.Windows.Forms.Timer roundTimer;
        private char currentMove = ' ';
        private int roundTimeMs = 3000;
        private int timeRemaining;
        private bool gameStarted = false;
        private bool isAttacker = false;

        public Form1()
        {
            InitializeComponent();

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            roundTimer = new System.Windows.Forms.Timer();
            roundTimer.Interval = 50;
            roundTimer.Tick += RoundTimer_Tick;

            if (btnReady != null)
            {
                btnReady.Click += BtnReady_Click;
            }

            // Wire up the custom Paint events for advanced GDI+ rotation & coloring
            panelW.Paint += (s, e) => DrawIcon(e.Graphics, panelW.ClientSize, 'W');
            panelA.Paint += (s, e) => DrawIcon(e.Graphics, panelA.ClientSize, 'A');
            panelS.Paint += (s, e) => DrawIcon(e.Graphics, panelS.ClientSize, 'S');
            panelD.Paint += (s, e) => DrawIcon(e.Graphics, panelD.ClientSize, 'D');
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 5643);
                stream = client.GetStream();

                ListenToServerAsync();
            }
            catch (Exception)
            {
                lblStatus.Text = "Connection failed: Is the server running?";
                if (btnReady != null) btnReady.Enabled = false;
            }
        }

        private async void BtnReady_Click(object sender, EventArgs e)
        {
            btnReady.Visible = false;
            prgTimer.Visible = true;
            lblStatus.Text = "Waiting for opponent to ready up...";

            byte[] buffer = Encoding.UTF8.GetBytes("READY");
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async void ListenToServerAsync()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (!gameStarted && serverMessage.Contains("LOBBY"))
                {
                    lblStatus.Text = "Connected! Click READY when you are set.";
                    continue;
                }

                gameStarted = true;
                isAttacker = serverMessage.Contains("[ATTACKER]");

                lblStatus.Text = serverMessage;

                // Redraw panels to switch between gloves and arrows based on role
                RefreshPanels();
                StartRound();
            }
        }

        private void RefreshPanels()
        {
            panelW.Invalidate();
            panelA.Invalidate();
            panelS.Invalidate();
            panelD.Invalidate();
        }

        private void DrawIcon(Graphics g, Size size, char dir)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Color: Blue if currently held down, Gray if idle
            bool isPressed = (currentMove == dir);
            Color brushColor = isPressed ? Color.Blue : Color.Gray;

            // Correct rotation angles for all 4 directions (0 = Up, 90 = Right, 180 = Down, 270 = Left)
            float angle = 0f;
            if (dir == 'W') angle = 0f;       // Up
            else if (dir == 'D') angle = 90f;   // Right
            else if (dir == 'S') angle = 180f;  // Down
            else if (dir == 'A') angle = 270f;  // Left

            // Use boxing gloves for the attacker, and the uniform '▲' character for the defender
            string text = isAttacker ? "🥊" : "▲";

            using (Font font = new Font("Segoe UI Emoji", 32, FontStyle.Regular))
            using (Brush brush = new SolidBrush(brushColor))
            {
                // Move origin to the center of the panel, rotate, and draw perfectly centered
                g.TranslateTransform(size.Width / 2f, size.Height / 2f);
                g.RotateTransform(angle);

                SizeF textSize = g.MeasureString(text, font);
                g.DrawString(text, font, brush, -textSize.Width / 2f, -textSize.Height / 2f);

                g.ResetTransform();
            }
        }

        private void StartRound()
        {
            timeRemaining = roundTimeMs;
            prgTimer.Maximum = roundTimeMs;
            prgTimer.Value = roundTimeMs;
            roundTimer.Start();
        }

        private async void RoundTimer_Tick(object sender, EventArgs e)
        {
            timeRemaining -= roundTimer.Interval;

            if (timeRemaining <= 0)
            {
                roundTimer.Stop();
                prgTimer.Value = 0;

                string moveToSend = currentMove == ' ' ? "NONE" : currentMove.ToString();
                byte[] buffer = Encoding.UTF8.GetBytes(moveToSend);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                lblStatus.Text = "Move locked! Waiting for opponent...";
            }
            else
            {
                prgTimer.Value = timeRemaining;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameStarted) return;

            if (e.KeyCode == Keys.W) { currentMove = 'W'; RefreshPanels(); }
            else if (e.KeyCode == Keys.S) { currentMove = 'S'; RefreshPanels(); }
            else if (e.KeyCode == Keys.A) { currentMove = 'A'; RefreshPanels(); }
            else if (e.KeyCode == Keys.D) { currentMove = 'D'; RefreshPanels(); }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && currentMove == 'W') { currentMove = ' '; RefreshPanels(); }
            else if (e.KeyCode == Keys.S && currentMove == 'S') { currentMove = ' '; RefreshPanels(); }
            else if (e.KeyCode == Keys.A && currentMove == 'A') { currentMove = ' '; RefreshPanels(); }
            else if (e.KeyCode == Keys.D && currentMove == 'D') { currentMove = ' '; RefreshPanels(); }
        }
    }
}