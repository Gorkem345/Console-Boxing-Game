using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

Console.WriteLine("Starting Boxing Game Server...");

TcpListener listener = new TcpListener(IPAddress.Any, 5643);
listener.Start();
Console.WriteLine("Waiting for 2 players to connect on port 5643...");

using TcpClient player1 = await listener.AcceptTcpClientAsync();
Console.WriteLine("Player 1 connected!");
NetworkStream stream1 = player1.GetStream();

using TcpClient player2 = await listener.AcceptTcpClientAsync();
Console.WriteLine("Player 2 connected!");
NetworkStream stream2 = player2.GetStream();

// Game State Variables
int p1Score = 0;
int p2Score = 0;
bool p1IsAttacker = true;
string lastRoundResult = "Game Start! Player 1 starts as the Attacker.";

// The Main Game Loop
while (true)
{
    // 1. Build the status message for each player
    string p1Role = p1IsAttacker ? "[ATTACKING]" : "[DEFENDING]";
    string p2Role = !p1IsAttacker ? "[ATTACKING]" : "[DEFENDING]";

    string p1Prompt = $"\n{lastRoundResult}\nScore -> P1: {p1Score} | P2: {p2Score}\n{p1Role} Enter W, S, A, or D: ";
    string p2Prompt = $"\n{lastRoundResult}\nScore -> P1: {p1Score} | P2: {p2Score}\n{p2Role} Enter W, S, A, or D: ";

    // 2. Send prompts to both players
    await SendMessageAsync(stream1, p1Prompt);
    await SendMessageAsync(stream2, p2Prompt);

    // 3. Wait for both players to submit their moves simultaneously 
    Task<string> p1Task = ReceiveMessageAsync(stream1);
    Task<string> p2Task = ReceiveMessageAsync(stream2);
    await Task.WhenAll(p1Task, p2Task);

    string p1Move = p1Task.Result.ToUpper();
    string p2Move = p2Task.Result.ToUpper();

    // 4. Calculate the logic
    string attackerMove = p1IsAttacker ? p1Move : p2Move;
    string defenderMove = p1IsAttacker ? p2Move : p1Move;
    bool attackerScored = false;

    if (attackerMove == "NONE")
    {
        // Attacker did nothing. Defender is safe.
        lastRoundResult = $"DEFENDER SAFE! Attacker froze. (A: {attackerMove}, D: {defenderMove}). Roles swapped!";
        p1IsAttacker = !p1IsAttacker;
    }
    else if (defenderMove == "NONE")
    {
        // Defender did nothing while Attacker attacked. Free hit!
        attackerScored = true;
        lastRoundResult = $"PUNISH! Defender froze. (A: {attackerMove}, D: {defenderMove}). Attacker scores!";
    }
    else if (attackerMove == defenderMove)
    {
        // Both picked a direction, and it's a match.
        attackerScored = true;
        lastRoundResult = $"HIT! (A: {attackerMove}, D: {defenderMove}). Attacker scores!";
    }
    else
    {
        // Both picked a direction, but they don't match. Blocked!
        lastRoundResult = $"BLOCKED! (A: {attackerMove}, D: {defenderMove}). Roles swapped!";
        p1IsAttacker = !p1IsAttacker;
    }

    // Apply the score if the attacker successfully landed a hit
    if (attackerScored)
    {
        if (p1IsAttacker) p1Score++;
        else p2Score++;
    }
}

// Helper: Send a string over the network
static async Task SendMessageAsync(NetworkStream stream, string message)
{
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    await stream.WriteAsync(buffer, 0, buffer.Length);
}

// Helper: Receive a network message and convert it to a string
static async Task<string> ReceiveMessageAsync(NetworkStream stream)
{
    byte[] buffer = new byte[1024];
    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
}