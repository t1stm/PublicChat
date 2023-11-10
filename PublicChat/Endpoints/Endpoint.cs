using vtortola.WebSockets;

namespace PublicChat.Endpoints;

public abstract class Endpoint
{
    public abstract string Location { get; set; }
    public abstract Task HandleConnection(WebSocket web_socket, string[] split_request, CancellationToken? cancellation_token);
}