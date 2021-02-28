using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Allard.Configinator.Api.Commands.ViewModels;

namespace Allard.Configinator.Api
{
    public class LinkHelper
    {
        private readonly string baseAddress = "/api/v1/";
        private const string Self = "self";
        private const string SchemaType = "schemaType";
        private const string SchemaTypes = "schemaTypes";
        private const string Realm = "realm";
        private const string Realms = "realms";
        private const string ConfigurationSections = "configurationSections";
        private const string ConfigurationSection = "configurationSection";
        private const string Root = "root";

        public LinkBuilder CreateBuilder()
        {
            return new(baseAddress);
        }

        public class LinkBuilder
        {
            private readonly string baseAddress;
            private readonly List<Link> links = new();

            public LinkBuilder(string baseAddress)
            {
                this.baseAddress = baseAddress;
            }

            private LinkBuilder Add(string rel, HttpMethod method, params string[] parts)
            {
                var address = string.Join("/", parts.Select(WebUtility.UrlEncode));
                links.Add(new Link(baseAddress + address, rel, method.ToString()));
                return this;
            }

            public LinkBuilder AddSchemaType(string typeId, bool self = false) =>
                Add(Rel(self, SchemaType), HttpMethod.Get, SchemaTypes, typeId);

            public LinkBuilder AddSchemaTypes(bool self = false) =>
                Add(Rel(self, SchemaTypes), HttpMethod.Get, SchemaTypes);

            public LinkBuilder AddRealms(bool self = false) =>
                Add(Rel(self, Realms), HttpMethod.Get, Realms);

            public LinkBuilder AddRealm(string name, bool self = false) =>
                Add(Rel(self, Realm), HttpMethod.Get, Realms, name);

            public LinkBuilder AddConfigurationSection(string realm, string configurationSection, bool self = false) =>
                Add(Rel(self, ConfigurationSection), HttpMethod.Get, "realms", realm, "sections", configurationSection);

            public LinkBuilder AddRoot(bool self = false)
            {
                links.Add(new Link(
                    // trim the / from the end of the base address.
                    baseAddress.Substring(0, baseAddress.Length - 1),
                    HttpMethod.Get.ToString(),
                    Rel(self, Root)
                ));
                return this;
            }

            private static string Rel(bool isSelf, string rel)
            {
                return isSelf ? Self : rel;
            }

            public IEnumerable<Link> Build()
            {
                return links;
            }
        }

        public static void AddLinksToRealm(LinkHelper linkHelper, RealmViewModel realm, bool selfRealm)
        {
            realm.Links = linkHelper
                .CreateBuilder()
                .AddRealm(realm.Name, selfRealm)
                .Build()
                .ToList();
            foreach (var cs in realm.ConfigurationSections)
            {
                cs.Links = linkHelper
                    .CreateBuilder()
                    .AddConfigurationSection(realm.Name, cs.Name)
                    .Build()
                    .ToList();
            }
        }
    }
}