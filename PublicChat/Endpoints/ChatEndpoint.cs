using vtortola.WebSockets;

namespace PublicChat;

public class ChatEndpoint : Endpoint
{
    public override string Location { get; set; } = "Chat";
    public override async Task HandleConnection(WebSocket web_socket, CancellationToken? cancellation_token)
    {
        
    }
}