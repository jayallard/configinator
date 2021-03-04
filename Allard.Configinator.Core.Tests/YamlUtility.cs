using System;
using System.IO;
using Allard.Configinator.Core.Tests.Unit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Allard.Configinator.Core.Tests
{
    public static class YamlUtility
    {
        public static OrganizationDto GetOrgFromFile(string shortName)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "__TestFiles", shortName + ".yaml");
            var doc = File.ReadAllText(file);

            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<OrganizationDto>(doc);
        }
    }
}