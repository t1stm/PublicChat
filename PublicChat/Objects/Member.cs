using System.Net;
using vtortola.WebSockets;
using WebSocket = vtortola.WebSockets.WebSocket;

namespace PublicChat.Objects;

public class Member
{
    private readonly SemaphoreSlim Write_Lock = new(1, 1);
    public string? Username;
    public long ID;
    public SocketAddress? IP_Address;
    public readonly WebSocket WebSocket;

    public string StyledUsername => Username ?? $"Anonymous {ID}";

    public Member(WebSocket webSocket)
    {
        WebSocket = webSocket;
    }

    public async Task WriteString(string message)
    {
        try
        {
            await Write_Lock.WaitAsync();
            await WebSocket.WriteStringAsync(message);
        }
        finally
        {
            Write_Lock.Release();
        }
    }
}