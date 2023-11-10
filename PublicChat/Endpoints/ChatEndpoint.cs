using PublicChat.Objects;
using vtortola.WebSockets;

namespace PublicChat.Endpoints;

public class ChatEndpoint : Endpoint
{
    public override string Location { get; set; } = "Chat";
    private readonly Dictionary<string, ChatRoom> Rooms = new();

    public ChatEndpoint()
    {
        // Adds a generic public chat room.
        Rooms.Add(string.Empty, new ChatRoom());
    }
    public override async Task HandleConnection(WebSocket web_socket, string[] split_request, CancellationToken? cancellation_token)
    {
        ChatRoom? room = null;
        string? username = null;
        
        switch (split_request.Length)
        {
            // Generic chat room connection.
            case 2:
                room = Rooms[string.Empty];
                username = split_request[1];
                break;
            // Selected chat room.
            case 3:
                var room_name = split_request[1];
                var found_room = Rooms.TryGetValue(room_name, out var temporary_room);
                if (!found_room || temporary_room is null)
                {
                    room = new ChatRoom();
                    Rooms.TryAdd(room_name, room);
                }
                else room = temporary_room;
                username = split_request[2];
                break;
        }

        if (room == null)
        {
            throw new Exception("Room is null in the chat endpoint connection handler.");
        }
        
        var member = new Member(web_socket)
        {
            ID = room.GetConnectionID(),
            IP_Address = web_socket.RemoteEndpoint.Serialize(),
            Username = username ?? "Anonymous"
        };

        room.AddMember(member);

        try
        {
            while (web_socket.IsConnected && !(cancellation_token?.IsCancellationRequested ?? false))
            {
                var message = await web_socket.ReadStringAsync(cancellation_token ?? CancellationToken.None);
                await Console.Out.WriteLineAsync($"[ChatEndpoint] Got message: \'{message}\'");
                // Client has disconnected. Break the loop.
                if (message == null) break;
                if (string.IsNullOrWhiteSpace(message)) continue;

                var broadcast_message = new Message
                {
                    Sender = member.Username,
                    SendTime = DateTime.Now,
                    Text = message
                };
                
                await room.Broadcast(broadcast_message);
            }
        }
        finally
        {
            room.RemoveMember(member);
        }
    }
}