using Loggy.Core.Db;
using Loggy.Core.Models.Events;

namespace Loggy.Api.Endpoints.Events;

public static class EventsEndpoint
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/events");
        group.MapPost("/", async (EventDto eventDto, LoggyDbContext db, CancellationToken cancellationToken) =>
        {
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Type = eventDto.Type,
                Timestamp = DateTime.UtcNow,
                Amount = eventDto.Amount,
                Message = eventDto.Message
            };

            db.Events.Add(@event);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/events/{@event.Id}", @event);
        });

        group.MapGet("/", async (LoggyDbContext db, CancellationToken cancellationToken) =>
        {
            return Results.Ok(new { message = "todo;" });
        });
    }
}