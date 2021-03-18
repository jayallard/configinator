using System.IO;
using System.Text;
using System.Text.Json;

namespace Allard.Configinator.Core
{
    public static class Utilities
    {
        // todo: temporary. bite the bullet and go full json
        public static string ConvertToString(this JsonDocument document)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true});
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}