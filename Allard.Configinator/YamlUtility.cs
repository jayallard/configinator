using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator
{
    public static class YamlUtility
    {
        public static async Task<IEnumerable<YamlDocument>> GetYamlFromFile(params string[] relativeFileName)
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(relativeFileName));
            var yaml = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);
            using var reader = new StringReader(yaml);
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            return yamlStream.Documents;
        }
    }
}