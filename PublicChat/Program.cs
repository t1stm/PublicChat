using System.Net;
using PublicChat;
using PublicChat.Endpoints;

var port = 11337;
var port_string = Environment.GetEnvironmentVariable("PORT");
if (port_string is not null && int.TryParse(port_string, out var temporary_port))
{
    port = temporary_port;
} 

var local_ip = new IPEndPoint(IPAddress.Any, port);
var websocket_server = new WebSocketServer(local_ip);

var chat_endpoint = new ChatEndpoint();
websocket_server.RegisterEndpoint(chat_endpoint);

await websocket_server.Start();