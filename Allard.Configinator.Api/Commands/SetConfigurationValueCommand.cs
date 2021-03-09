using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record SetConfigurationValueCommand : IRequest<Blah>
    {
        public string OrganizationName { get; init; }
        public string HabitatName { get; init; }
        public string RealmName { get; init; }
        public string ConfigurationSectionName { get; init; }
        public string? PreviousEtag { get; init; }
        public string Value { get; init; }
    }
}