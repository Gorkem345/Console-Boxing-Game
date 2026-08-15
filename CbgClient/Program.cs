using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

Console.WriteLine("Connecting to the referee...");
using TcpClient client = new TcpClient();
await client.ConnectAsync("127.0.0.1", 5643);
Console.WriteLine("Connected!");

NetworkStream stream = client.GetStream();

// The Client Game Loop
while (true)
{
    // 1. Wait for the server to send the round status and prompt
    string serverMessage = await ReceiveMessageAsync(stream);
    Console.Write(serverMessage); // Print the server's prompt

    // 2. Read a single valid keystroke (W, S, A, D) without needing Enter
    char validMove = ' ';
    while (true)
    {
        // 'intercept: true' prevents the console from typing the wrong keys on the screen
        var keyInfo = Console.ReadKey(intercept: true);
        char key = char.ToUpper(keyInfo.KeyChar);

        if (key == 'W' || key == 'S' || key == 'A' || key == 'D')
        {
            validMove = key;
            Console.WriteLine(validMove); // Print the valid key so the player sees their move
            break; // Exit the input loop
        }
    }

    // 3. Send the single character move back to the server
    await SendMessageAsync(stream, validMove.ToString());
}

// Helpers 
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