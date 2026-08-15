using System;
using System.Collections.Generic;
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

        private bool isGameOver = false;
        private bool hasWon = false;

        private Dictionary<char, int> directionHits = new Dictionary<char, int>
        {
            { 'W', 0 }, { 'A', 0 }, { 'S', 0 }, { 'D', 0 }
        };

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

            // GDI+ Renderers
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

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes("READY");
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch { }
        }

        private async void ListenToServerAsync()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0) break;

                string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (!gameStarted && serverMessage.Contains("LOBBY"))
                {
                    lblStatus.Text = "Connected! Click READY when you are set.";
                    continue;
                }

                gameStarted = true;

                // 1. Check for Game Over
                if (serverMessage.Contains("GAMEOVER:"))
                {
                    isGameOver = true;
                    hasWon = serverMessage.Contains("GAMEOVER:WIN");
                    roundTimer.Stop();
                    prgTimer.Visible = false;

                    string finalDisplay = serverMessage
                        .Replace("GAMEOVER:WIN\n", "")
                        .Replace("GAMEOVER:LOSE\n", "")
                        .Replace("HITS:W3,A3,S3,D3", "")
                        .Trim();

                    lblStatus.Text = finalDisplay;
                    RefreshPanels();
                    break;
                }

                // 2. Role Identification
                isAttacker = serverMessage.Contains("ROLE:ATTACKER");

                // 3. Dynamic Timer Duration Parsing
                if (serverMessage.Contains("TIME:"))
                {
                    int startIdx = serverMessage.IndexOf("TIME:") + 5;
                    int endIdx = serverMessage.IndexOf('\n', startIdx);
                    string timeStr = (endIdx != -1)
                        ? serverMessage.Substring(startIdx, endIdx - startIdx).Trim()
                        : serverMessage.Substring(startIdx).Trim();

                    if (int.TryParse(timeStr, out int parsedTime))
                    {
                        roundTimeMs = parsedTime;
                    }
                }

                // 4. Parse Hits
                if (serverMessage.Contains("HITS:"))
                {
                    int startIdx = serverMessage.IndexOf("HITS:") + 5;
                    int endIdx = serverMessage.IndexOf('\n', startIdx);
                    string hitsStr = (endIdx != -1)
                        ? serverMessage.Substring(startIdx, endIdx - startIdx).Trim()
                        : serverMessage.Substring(startIdx).Trim();

                    var tokens = hitsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        if (token.Length >= 2 && "WASD".Contains(token[0].ToString()))
                        {
                            if (int.TryParse(token.Substring(1), out int count))
                            {
                                directionHits[token[0]] = count;
                            }
                        }
                    }
                }

                // 5. Clean Status Display Text
                string displayMessage = serverMessage
                    .Replace("ROLE:ATTACKER\n", "")
                    .Replace("ROLE:DEFENDER\n", "");

                int hIdx = displayMessage.IndexOf("HITS:");
                if (hIdx != -1)
                {
                    int nIdx = displayMessage.IndexOf('\n', hIdx);
                    displayMessage = nIdx != -1 ? displayMessage.Remove(hIdx, nIdx - hIdx + 1) : displayMessage.Substring(0, hIdx);
                }

                int tIdx = displayMessage.IndexOf("TIME:");
                if (tIdx != -1)
                {
                    int nIdx = displayMessage.IndexOf('\n', tIdx);
                    displayMessage = nIdx != -1 ? displayMessage.Remove(tIdx, nIdx - tIdx + 1) : displayMessage.Substring(0, tIdx);
                }

                lblStatus.Text = displayMessage.Trim();
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

            // Endgame View
            if (isGameOver)
            {
                using (Font font = new Font("Segoe UI Emoji", 30, FontStyle.Regular))
                using (Brush brush = new SolidBrush(hasWon ? Color.Gold : Color.Red))
                {
                    g.TranslateTransform(size.Width / 2f, size.Height / 2f);
                    string text = hasWon ? "🏆" : "💀";
                    SizeF textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, -textSize.Width / 2f, -textSize.Height / 2f);
                    g.ResetTransform();
                }
                return;
            }

            int hits = directionHits[dir];
            bool isBroken = hits >= 3;
            bool isPressed = (currentMove == dir);

            float angle = 0f;
            if (dir == 'W') angle = 0f;
            else if (dir == 'D') angle = 90f;
            else if (dir == 'S') angle = 180f;
            else if (dir == 'A') angle = 270f;

            using (Font font = new Font("Segoe UI Emoji", 30, FontStyle.Regular))
            {
                if (isBroken)
                {
                    if (isAttacker)
                    {
                        using (Brush brush = new SolidBrush(Color.Gold))
                        {
                            g.TranslateTransform(size.Width / 2f, size.Height / 2f);
                            g.RotateTransform(angle);
                            string text = "🥊";
                            SizeF textSize = g.MeasureString(text, font);
                            g.DrawString(text, font, brush, -textSize.Width / 2f, -textSize.Height / 2f);
                            g.ResetTransform();
                        }
                    }
                    else
                    {
                        using (Brush brush = new SolidBrush(Color.Red))
                        {
                            g.TranslateTransform(size.Width / 2f, size.Height / 2f);
                            string text = "💀";
                            SizeF textSize = g.MeasureString(text, font);
                            g.DrawString(text, font, brush, -textSize.Width / 2f, -textSize.Height / 2f);
                            g.ResetTransform();
                        }
                    }
                }
                else
                {
                    Color brushColor = isPressed ? Color.Blue : Color.Gray;
                    string text = isAttacker ? "🥊" : "▲";

                    g.TranslateTransform(size.Width / 2f, size.Height / 2f);
                    g.RotateTransform(angle);

                    // 1. Draw Base Icon
                    using (Brush brush = new SolidBrush(brushColor))
                    {
                        SizeF textSize = g.MeasureString(text, font);
                        g.DrawString(text, font, brush, -textSize.Width / 2f, -textSize.Height / 2f);
                    }

                    // 2. Procedural Cracks
                    if (hits > 0)
                    {
                        using (Pen crackPen = new Pen(Color.FromArgb(230, 20, 20, 20), 2.2f))
                        {
                            crackPen.StartCap = LineCap.Round;
                            crackPen.EndCap = LineCap.Round;

                            // Stage 1: Single fracture
                            PointF[] crack1 = {
                                new PointF(-6, -14),
                                new PointF(-2, -5),
                                new PointF(-7, 4),
                                new PointF(5, 14)
                            };
                            g.DrawLines(crackPen, crack1);

                            // Stage 2: Branching fractures
                            if (hits >= 2)
                            {
                                PointF[] branch1 = {
                                    new PointF(-2, -5),
                                    new PointF(8, -3),
                                    new PointF(12, -10)
                                };
                                PointF[] branch2 = {
                                    new PointF(-7, 4),
                                    new PointF(-13, 8),
                                    new PointF(-15, 15)
                                };
                                g.DrawLines(crackPen, branch1);
                                g.DrawLines(crackPen, branch2);
                            }
                        }
                    }

                    g.ResetTransform();
                }
            }
        }

        private void StartRound()
        {
            if (isGameOver) return;

            currentMove = ' ';
            RefreshPanels();

            timeRemaining = roundTimeMs;
            prgTimer.Maximum = roundTimeMs;
            prgTimer.Value = roundTimeMs;
            roundTimer.Start();
        }

        private async void RoundTimer_Tick(object sender, EventArgs e)
        {
            if (isGameOver)
            {
                roundTimer.Stop();
                return;
            }

            timeRemaining -= roundTimer.Interval;

            if (timeRemaining <= 0)
            {
                roundTimer.Stop();
                prgTimer.Value = 0;

                try
                {
                    string moveToSend = currentMove == ' ' ? "NONE" : currentMove.ToString();
                    byte[] buffer = Encoding.UTF8.GetBytes(moveToSend);
                    await stream.WriteAsync(buffer, 0, buffer.Length);

                    lblStatus.Text = "Move locked! Waiting for opponent...";
                }
                catch { }
            }
            else
            {
                prgTimer.Value = Math.Max(0, timeRemaining);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameStarted || isGameOver) return;

            char pressed = ' ';
            if (e.KeyCode == Keys.W) pressed = 'W';
            else if (e.KeyCode == Keys.S) pressed = 'S';
            else if (e.KeyCode == Keys.A) pressed = 'A';
            else if (e.KeyCode == Keys.D) pressed = 'D';

            if (pressed != ' ' && directionHits[pressed] < 3)
            {
                currentMove = pressed;
                RefreshPanels();
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;

            char released = ' ';
            if (e.KeyCode == Keys.W) released = 'W';
            else if (e.KeyCode == Keys.S) released = 'S';
            else if (e.KeyCode == Keys.A) released = 'A';
            else if (e.KeyCode == Keys.D) released = 'D';

            if (released != ' ' && currentMove == released)
            {
                currentMove = ' ';
                RefreshPanels();
            }
        }
    }
}