using YamlDotNet.RepresentationModel;

namespace Allard.Configinator.Tests.Unit
{
    public static class ExtensionMethods
    {
        public static string StringValue(this YamlNode node, string name)
        {
            return (string) node[name];
        }
    }
}