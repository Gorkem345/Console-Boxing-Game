using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CbgDesktopClient // Ensure this matches your actual project name
{
    public partial class Form1 : Form
    {
        // Network variables
        private TcpClient client;
        private NetworkStream stream;

        // Game logic variables
        private System.Windows.Forms.Timer roundTimer;
        private char currentMove = ' '; // Tracks the currently held key
        private int roundTimeMs = 3000; // 3 seconds per round (Adjustable)
        private int timeRemaining;

        public Form1()
        {
            InitializeComponent();

            // Wire up all our events directly in code to avoid Designer crashes
            this.Load += Form1_Load;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            // Setup the round timer programmatically
            roundTimer = new System.Windows.Forms.Timer();
            roundTimer.Interval = 50; // Update the bar every 50 milliseconds for a smooth slide
            roundTimer.Tick += RoundTimer_Tick;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Connect to the server the moment the UI window opens
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5643);
            stream = client.GetStream();

            // Start an endless loop to listen for the referee's messages in the background
            ListenToServerAsync();
        }

        private async void ListenToServerAsync()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Server disconnected

                // Whenever the server sends a prompt, reset the time and start the bar
                StartRound();
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
                // Time is up! 
                roundTimer.Stop();
                prgTimer.Value = 0;

                // Send the move being held at this exact millisecond. If empty, send "NONE"
                string moveToSend = currentMove == ' ' ? "NONE" : currentMove.ToString();
                byte[] buffer = Encoding.UTF8.GetBytes(moveToSend);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                // Shrink the bar
                prgTimer.Value = timeRemaining;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // 1. Immediately turn everything gray to wipe out older key presses
            lblW.ForeColor = Color.Gray;
            lblS.ForeColor = Color.Gray;
            lblA.ForeColor = Color.Gray;
            lblD.ForeColor = Color.Gray;

            // 2. Lock in the newest key press and turn only that arrow blue
            if (e.KeyCode == Keys.W) { currentMove = 'W'; lblW.ForeColor = Color.Blue; }
            else if (e.KeyCode == Keys.S) { currentMove = 'S'; lblS.ForeColor = Color.Blue; }
            else if (e.KeyCode == Keys.A) { currentMove = 'A'; lblA.ForeColor = Color.Blue; }
            else if (e.KeyCode == Keys.D) { currentMove = 'D'; lblD.ForeColor = Color.Blue; }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // If the key the player just let go of matches the one they were holding, clear the move
            if (e.KeyCode == Keys.W && currentMove == 'W') { currentMove = ' '; lblW.ForeColor = Color.Gray; }
            else if (e.KeyCode == Keys.S && currentMove == 'S') { currentMove = ' '; lblS.ForeColor = Color.Gray; }
            else if (e.KeyCode == Keys.A && currentMove == 'A') { currentMove = ' '; lblA.ForeColor = Color.Gray; }
            else if (e.KeyCode == Keys.D && currentMove == 'D') { currentMove = ' '; lblD.ForeColor = Color.Gray; }
        }
    }
}