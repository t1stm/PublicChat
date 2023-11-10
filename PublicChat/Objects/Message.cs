using System.Text.Json.Serialization;

namespace PublicChat.Objects;

public struct Message
{
    [JsonInclude]
    public DateTime SendTime;
    [JsonInclude]
    public string Text;
    [JsonInclude]
    public string Sender;
}