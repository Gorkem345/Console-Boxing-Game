using System;
using System.Collections.Generic;
using System.Linq;
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

        // Lobby Phase
        await SendMessageAsync(stream1, "LOBBY: Click READY to start!");
        await SendMessageAsync(stream2, "LOBBY: Click READY to start!");

        await ReceiveMessageAsync(stream1);
        await ReceiveMessageAsync(stream2);
        Console.WriteLine("Both players ready! Match begins.");

        int p1Score = 0;
        int p2Score = 0;
        int roundNumber = 1;
        bool p1IsAttacker = true;
        string lastRoundResult = "FIGHT START!";

        Dictionary<char, int> p1Damage = new Dictionary<char, int> { { 'W', 0 }, { 'A', 0 }, { 'S', 0 }, { 'D', 0 } };
        Dictionary<char, int> p2Damage = new Dictionary<char, int> { { 'W', 0 }, { 'A', 0 }, { 'S', 0 }, { 'D', 0 } };

        // Main Game Loop
        while (true)
        {
            // Knockout Check
            bool p1KnockedOut = p1Damage.Values.All(v => v >= 3);
            bool p2KnockedOut = p2Damage.Values.All(v => v >= 3);

            if (p1KnockedOut || p2KnockedOut)
            {
                bool p1Won = p2KnockedOut;

                string p1End = p1Won
                    ? $"VICTORY!\nYou won by K.O.!\nScore -> P1: {p1Score} | P2: {p2Score}\nGAMEOVER:WIN\nHITS:W3,A3,S3,D3"
                    : $"KNOCKOUT!\nYou were knocked out!\nScore -> P1: {p1Score} | P2: {p2Score}\nGAMEOVER:LOSE\nHITS:W3,A3,S3,D3";

                string p2End = !p1Won
                    ? $"VICTORY!\nYou won by K.O.!\nScore -> P1: {p1Score} | P2: {p2Score}\nGAMEOVER:WIN\nHITS:W3,A3,S3,D3"
                    : $"KNOCKOUT!\nYou were knocked out!\nScore -> P1: {p1Score} | P2: {p2Score}\nGAMEOVER:LOSE\nHITS:W3,A3,S3,D3";

                await SendMessageAsync(stream1, p1End);
                await SendMessageAsync(stream2, p2End);

                Console.WriteLine($"Match ended: {(p1Won ? "Player 1" : "Player 2")} Wins by K.O.!");
                await Task.Delay(3000);
                break;
            }

            // Progressive Round Speed Scaling
            int currentRoundTime = 3000;
            if (roundNumber > 15) currentRoundTime = 1000;
            else if (roundNumber > 5) currentRoundTime = 2000;

            Dictionary<char, int> defenderDamage = p1IsAttacker ? p2Damage : p1Damage;
            string hitsList = $"W{defenderDamage['W']},A{defenderDamage['A']},S{defenderDamage['S']},D{defenderDamage['D']}";

            string p1RoleTag = p1IsAttacker ? "ROLE:ATTACKER" : "ROLE:DEFENDER";
            string p2RoleTag = p1IsAttacker ? "ROLE:DEFENDER" : "ROLE:ATTACKER";

            string p1RoleTitle = p1IsAttacker ? "YOU ARE ATTACKING (Gloves)" : "YOU ARE DEFENDING (Arrows)";
            string p2RoleTitle = p1IsAttacker ? "YOU ARE DEFENDING (Arrows)" : "YOU ARE ATTACKING (Gloves)";

            string p1Prompt = $"{lastRoundResult}\n[Round {roundNumber}] Score -> P1: {p1Score} | P2: {p2Score}\n[Player 1] {p1RoleTitle}\n{p1RoleTag}\nHITS:{hitsList}\nTIME:{currentRoundTime}\n";
            string p2Prompt = $"{lastRoundResult}\n[Round {roundNumber}] Score -> P1: {p1Score} | P2: {p2Score}\n[Player 2] {p2RoleTitle}\n{p2RoleTag}\nHITS:{hitsList}\nTIME:{currentRoundTime}\n";

            await SendMessageAsync(stream1, p1Prompt);
            await SendMessageAsync(stream2, p2Prompt);

            string p1Move = await ReceiveMessageAsync(stream1);
            string p2Move = await ReceiveMessageAsync(stream2);

            string attackerMove = p1IsAttacker ? p1Move : p2Move;
            string defenderMove = p1IsAttacker ? p2Move : p1Move;
            bool attackerScored = false;

            if (attackerMove == "NONE")
            {
                lastRoundResult = "DEFENDER SAFE! Attacker froze. Roles swapped!";
                p1IsAttacker = !p1IsAttacker;
            }
            else if (defenderMove == "NONE")
            {
                attackerScored = true;
                lastRoundResult = $"PUNISH! Defender froze ({attackerMove} hit!). Attacker scores!";
            }
            else if (attackerMove == defenderMove)
            {
                attackerScored = true;
                lastRoundResult = $"HIT! ({attackerMove} landed!). Attacker scores!";
            }
            else
            {
                lastRoundResult = $"BLOCKED! (A:{attackerMove}, D:{defenderMove}). Roles swapped!";
                p1IsAttacker = !p1IsAttacker;
            }

            if (attackerScored && attackerMove.Length > 0 && attackerMove != "NONE")
            {
                char hitDir = attackerMove[0];
                if (p1IsAttacker)
                {
                    p1Score++;
                    if (p2Damage[hitDir] < 3) p2Damage[hitDir]++;
                }
                else
                {
                    p2Score++;
                    if (p1Damage[hitDir] < 3) p1Damage[hitDir]++;
                }
            }

            roundNumber++;
        }
    }

    static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch { }
    }

    static async Task<string> ReceiveMessageAsync(NetworkStream stream)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) return "NONE";
            return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        }
        catch
        {
            return "NONE";
        }
    }
}