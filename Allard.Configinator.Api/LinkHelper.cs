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

            public LinkBuilder Add(string rel, HttpMethod method, params string[] parts)
            {
                var address = string.Join("/", parts.Select(WebUtility.UrlEncode));
                links.Add(new Link
                {
                    Href = baseAddress + address,
                    Method = method.ToString(),
                    Rel = rel
                });

                return this;
            }

            public LinkBuilder AddTypeId(string typeId, bool self = false) => Add(self ? "self" : "typeId", HttpMethod.Get, "types", typeId);

            public LinkBuilder AddConfigurationSection(
                ConfigurationSectionId id,
                bool self = false)
            {
                Add(self ? "self" : "configSection", HttpMethod.Get, "realms", id.Realm, "sections",
                    id.Name);
                return this;
            }

            public IEnumerable<Link> Build()
            {
                return links;
            }
        }
    }
}