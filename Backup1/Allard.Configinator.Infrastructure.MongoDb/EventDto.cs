using System;
using System.Diagnostics;
using Allard.Configinator.Core.Ddd;
using Allard.Configinator.Core.Model;
using MongoDB.Bson;

namespace Allard.Configinator.Infrastructure.MongoDb
{
    // todo: wip - slapping it together to get it to work.
    // redundant org name and id
    [DebuggerDisplay("{EventName}")]
    public record EventDto(
        BsonObjectId Id,
        string DbTransactionId,
        string EventId,
        OrganizationId OrganizationId,
        DateTime EventDate,
        string EventName,
        DomainEvent Event);
}