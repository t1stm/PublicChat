using System.Text.Json.Serialization;

namespace PublicChat.Objects;

public struct ResponseError
{
    [JsonInclude]
    public string? ErrorText;
}