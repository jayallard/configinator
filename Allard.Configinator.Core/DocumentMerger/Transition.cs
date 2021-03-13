using System.Text.Json.Serialization;

namespace Allard.Configinator.Core.DocumentMerger
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Transition
    {
        Set,
        SetToSameValue,
        Delete,
        DoesntExist,
        Inherit
    }
}