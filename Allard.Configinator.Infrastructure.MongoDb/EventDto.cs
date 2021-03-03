using System;
using Allard.Configinator.Core.Ddd;
using MongoDB.Bson;

namespace Allard.Configinator.Infrastructure.MongoDb
{
    public record EventDto(
        BsonObjectId Id,
        string EventId,
        string OrganizationId,
        DateTime EventDate,
        string EventName,
        DomainEvent Event);
}