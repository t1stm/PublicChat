using vtortola.WebSockets;

namespace PublicChat;

public abstract class Endpoint
{
    public abstract string Location { get; set; }
    public abstract Task HandleConnection(WebSocket web_socket, CancellationToken? cancellation_token);
}