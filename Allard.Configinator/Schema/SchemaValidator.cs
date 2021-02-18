using System;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Schema
{
    public class SchemaValidator
    {
        private readonly SchemaParser parser;

        public SchemaValidator(SchemaParser parser)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public async Task Validate(YamlNode doc)
        {
        }
    }
}