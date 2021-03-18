using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Blazor.Shared
{
    public record Link(string Href, string Rel, string Method);

    public class LinkHelper
    {
        private const string Self = "self";
        private const string SchemaType = "schemaType";
        private const string SchemaTypes = "schemaTypes";
        private const string Realm = "realm";
        private const string Realms = "realms";
        private const string ConfigurationSection = "configurationSection";
        private const string Root = "root";
        private readonly string BaseAddress = "/api/v1/";

        public LinkBuilder CreateBuilder()
        {
            return new(BaseAddress);
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

            public LinkBuilder AddConfigurationSection(RealmId realmId, SectionId sectionId, bool self = false)
            {
                return
                    Add(Rel(self, ConfigurationSection), HttpMethod.Get, "realms", realmId.Id, "sections",
                            sectionId.Id)
                        .Add(Rel(false, "valueRaw"), HttpMethod.Get, "realms", realmId.Id, "sections", sectionId.Id,
                            "value-raw", "{habitat}")
                        .Add(Rel(false, "valueResolved"), HttpMethod.Get, "realms", realmId.Id, "sections",
                            sectionId.Id,
                            "value-resolved", "{habitat}")
                        .Add(Rel(false, "valueExplained"), HttpMethod.Get, "realms", realmId.Id, "sections",
                            sectionId.Id,
                            "value-explained", "{habitat}");
            }

            public LinkBuilder AddRoot(bool self = false)
            {
                links.Add(new Link(
                    // trim the / from the end of the base address.
                    baseAddress[..^1],
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