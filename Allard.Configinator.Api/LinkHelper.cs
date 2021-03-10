using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Allard.Configinator.Api.Commands.ViewModels;

namespace Allard.Configinator.Api
{
    public class LinkHelper
    {
        private const string Self = "self";
        private const string SchemaType = "schemaType";
        private const string SchemaTypes = "schemaTypes";
        private const string Realm = "realm";
        private const string Realms = "realms";
        private const string ConfigurationSections = "configurationSections";
        private const string ConfigurationSection = "configurationSection";
        private const string Root = "root";
        private const string BaseAddress = "/api/v1/";

        public LinkBuilder CreateBuilder()
        {
            return new(BaseAddress);
        }

        public static void AddLinksToRealm(LinkHelper linkHelper, RealmViewModel realm, bool selfRealm)
        {
            realm.Links = linkHelper
                .CreateBuilder()
                .AddRealm(realm.RealmName, selfRealm)
                .Build()
                .ToList();
            foreach (var cs in realm.ConfigurationSections)
                cs.Links = linkHelper
                    .CreateBuilder()
                    .AddConfigurationSection(realm.RealmName, cs.ConfigurationSectionId.Name)
                    .Build()
                    .ToList();
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

            public LinkBuilder AddSchemaType(string typeId, bool self = false)
            {
                return Add(Rel(self, SchemaType), HttpMethod.Get, SchemaTypes, typeId);
            }

            public LinkBuilder AddSchemaTypes(bool self = false)
            {
                return Add(Rel(self, SchemaTypes), HttpMethod.Get, SchemaTypes);
            }

            public LinkBuilder AddRealms(bool self = false)
            {
                return Add(Rel(self, Realms), HttpMethod.Get, Realms);
            }

            public LinkBuilder AddRealm(string name, bool self = false)
            {
                return Add(Rel(self, Realm), HttpMethod.Get, Realms, name);
            }

            public LinkBuilder AddConfigurationSection(string realm, string configurationSection, bool self = false)
            {
                return
                    Add(Rel(self, ConfigurationSection), HttpMethod.Get, "realms", realm, "sections",
                            configurationSection)
                        .Add(Rel(false, "valueRaw"), HttpMethod.Get, "realms", realm, "sections", configurationSection,
                            "value-raw", "{habitat}")
                        .Add(Rel(false, "valueResolved"), HttpMethod.Get, "realms", realm, "sections",
                            configurationSection,
                            "value-resolved", "{habitat}")
                        .Add(Rel(false, "valueExplained"), HttpMethod.Get, "realms", realm, "sections",
                            configurationSection,
                            "value-explained", "{habitat}");
            }

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

            public List<Link> Build()
            {
                return links;
            }
        }
    }
}