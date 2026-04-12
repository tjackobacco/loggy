using Loggy.Core.Db;
using Loggy.Core.Models.Events;
using Microsoft.EntityFrameworkCore;

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

        group.MapGet("/", async (EventType? type,
            DateTime? from,
            DateTime? to,
            string? message,
            int limit,
            LoggyDbContext db,
            CancellationToken cancellationToken) =>
        {
            var query = db.Events.AsNoTracking();

            if (type is not null)
            {
                query = query.Where(e => e.Type == type);
            }
            if (from is not null)
            {
                query = query.Where(e => e.Timestamp >= from);
            }
            if (to is not null)
            {
                query = query.Where(e => e.Timestamp <= to);
            }
            if (!string.IsNullOrEmpty(message))
            {
                query = query.Where(e => e.Message.Contains(message));
            }

            var takeLimit = Math.Max(limit, 99);

            var res = await query
            .Take(takeLimit)
            .ToListAsync(cancellationToken);
        });
    }
}