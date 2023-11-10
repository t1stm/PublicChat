using System.Net;
using System.Text.Json;
using PublicChat.Endpoints;
using PublicChat.Objects;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace PublicChat;

public class WebSocketServer
{
    private readonly WebSocketListener Listener;
    private readonly Dictionary<string, Endpoint> Endpoints = new();
    
    public WebSocketServer(IPEndPoint listen_address)
    {
        var options = new WebSocketListenerOptions();
        options.Standards.RegisterRfc6455();
        options.Logger = new DebugLogger();
        options.PingMode = PingMode.BandwidthSaving;
        options.PingTimeout = new TimeSpan(0, 1, 0);
        
        Listener = new WebSocketListener(listen_address, options);
    }

    public async Task Start(CancellationToken? cancellation_token = null)
    {
        var token = cancellation_token ?? CancellationToken.None;
        await Listener.StartAsync();
        await Console.Out.WriteLineAsync("[WebSocketServer] Started successfully. Entering update loop.");
        await ServerUpdateLoop(token);
    }

    private async Task ServerUpdateLoop(CancellationToken cancellation_token)
    {
        while (!cancellation_token.IsCancellationRequested)
        {
            try
            {
                var ws = await Listener.AcceptWebSocketAsync(cancellation_token).ConfigureAwait(false);
                if (ws == null) continue;

                async void ConnectionHandle() => await HandleConnectionAsync(ws, cancellation_token).ConfigureAwait(false);

                var task = new Task(ConnectionHandle);
                task.Start();
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(
                    $"[WebSocketServer]: Error in connection handler: \'{e.GetBaseException().Message}\'");
            }
        }
    }

    private static async Task CloseWithMessage(WebSocket ws, string message)
    {
        var error = new ResponseError
        {
            ErrorText = message
        };
        await ws.WriteStringAsync(JsonSerializer.Serialize(error));
        await ws.CloseAsync();
        await Console.Out.WriteLineAsync($"[WebSocketServer] Closing connection \'{ws.HttpRequest} | {ws.RemoteEndpoint}\' with message: \'{message}\'");
    }
    
    private async Task HandleConnectionAsync(WebSocket ws, CancellationToken cancellation_token)
    {
        var request_url = ws.HttpRequest.RequestUri.ToString();
        await Console.Out.WriteLineAsync($"[WebSocketServer] Got connection: URI: \'{ws.HttpRequest}\' " +
                                         $"Endpoint: \'{ws.RemoteEndpoint}\'");
        if (string.IsNullOrEmpty(request_url))
        {
            await CloseWithMessage(ws, "Invalid Request.");
            return;
        }

        var pad = request_url[1..];
        if (string.IsNullOrEmpty(pad))
        {
            await CloseWithMessage(ws, "Invalid Request.");
            return;
        }

        var split = pad.Split('/');
        if (split.Length < 2)
        {
            await CloseWithMessage(ws, "Invalid Request.");
            return;
        }

        var first_split = split[0];
        var found_endpoint = Endpoints.TryGetValue(first_split, out var endpoint);
        if (!found_endpoint || endpoint is null || string.IsNullOrEmpty(string.Join('/', split[1..])))
        {
            await CloseWithMessage(ws, "Endpoint not found.");
            return;
        }

        await endpoint.HandleConnection(ws, split, cancellation_token);
    }

    public void RegisterEndpoint(Endpoint endpoint)
    {
        var endpoint_type = endpoint.GetType();
        var name = endpoint_type.Name;
        Endpoints.Add(endpoint.Location, endpoint);
        Console.Out.WriteLineAsync($"[WebSocketServer] Registered endpoint \'{name}\'");
    }
}