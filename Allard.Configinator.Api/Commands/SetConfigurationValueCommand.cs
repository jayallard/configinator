using MediatR;

namespace Allard.Configinator.Api.Commands
{
    public record SetConfigurationValueCommand : IRequest<Blah>
    {
        public string? PreviousEtag { get; init; }
        public string Habitat { get; init; }
        public string Realm { get; init; }
        public string ConfigurationSection { get; init; }
        public string Value { get; init; }
    }
}