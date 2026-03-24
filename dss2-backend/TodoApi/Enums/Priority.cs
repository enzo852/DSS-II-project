using System.Text.Json.Serialization;

namespace TodoApi.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    low,
    medium,
    high
}