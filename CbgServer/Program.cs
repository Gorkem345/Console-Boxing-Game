using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5643);
        server.Start();
        Console.WriteLine("Boxing Server started on port 5643. Waiting for 2 players...");

        TcpClient player1 = await server.AcceptTcpClientAsync();
        Console.WriteLine("Player 1 connected!");
        TcpClient player2 = await server.AcceptTcpClientAsync();
        Console.WriteLine("Player 2 connected!");

        NetworkStream stream1 = player1.GetStream();
        NetworkStream stream2 = player2.GetStream();

        // 1. LOBBY PHASE: Wait for both players to click Ready
        Console.WriteLine("Waiting for players to ready up...");
        await SendMessageAsync(stream1, "LOBBY: Click READY to start!");
        await SendMessageAsync(stream2, "LOBBY: Click READY to start!");

        string p1Status = await ReceiveMessageAsync(stream1);
        string p2Status = await ReceiveMessageAsync(stream2);

        Console.WriteLine("Both players are ready! Starting match...");

        int p1Score = 0;
        int p2Score = 0;
        bool p1IsAttacker = true;
        string lastRoundResult = "FIGHT START!";

        // 2. MAIN GAME LOOP
        while (true)
        {
            string p1Role = p1IsAttacker ? "[ATTACKER]" : "[DEFENDER]";
            string p2Role = p1IsAttacker ? "[DEFENDER]" : "[ATTACKER]";

            string p1Prompt = $"{lastRoundResult}\nScore -> P1: {p1Score} | P2: {p2Score}\n{p1Role} Your move:";
            string p2Prompt = $"{lastRoundResult}\nScore -> P1: {p1Score} | P2: {p2Score}\n{p2Role} Your move:";

            await SendMessageAsync(stream1, p1Prompt);
            await SendMessageAsync(stream2, p2Prompt);

            string p1Move = await ReceiveMessageAsync(stream1);
            string p2Move = await ReceiveMessageAsync(stream2);

            // Calculate logic
            string attackerMove = p1IsAttacker ? p1Move : p2Move;
            string defenderMove = p1IsAttacker ? p2Move : p1Move;
            bool attackerScored = false;

            if (attackerMove == "NONE")
            {
                lastRoundResult = $"DEFENDER SAFE! Attacker froze. Roles swapped!";
                p1IsAttacker = !p1IsAttacker;
            }
            else if (defenderMove == "NONE")
            {
                attackerScored = true;
                lastRoundResult = $"PUNISH! Defender froze. Attacker scores!";
            }
            else if (attackerMove == defenderMove)
            {
                attackerScored = true;
                lastRoundResult = $"HIT! ({attackerMove}) Attacker scores!";
            }
            else
            {
                lastRoundResult = $"BLOCKED! Roles swapped!";
                p1IsAttacker = !p1IsAttacker;
            }

            if (attackerScored)
            {
                if (p1IsAttacker) p1Score++;
                else p2Score++;
            }
        }
    }

    static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(buffer, 0, buffer.Length);
    }

    static async Task<string> ReceiveMessageAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }
}