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
                AccountId = eventDto.AccountId,
                Type = eventDto.Type,
                Timestamp = DateTime.UtcNow,
                Amount = eventDto.Amount,
                Message = eventDto.Message
            };

            db.Events.Add(@event);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/events/{@event.Id}", @event);
        });

        group.MapGet("/", async (string? accountId,
            EventType? type,
            DateTime? from,
            DateTime? to,
            string? message,
            int? limit,
            LoggyDbContext db,
            CancellationToken cancellationToken) =>
        {
            var query = db.Events.AsNoTracking();

            if (!string.IsNullOrEmpty(accountId))
            {
                query = query.Where(e => e.AccountId == accountId);
            }
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

            var takeLimit = Math.Max(limit ?? 0, 99);

            var res = await query
            .OrderBy(x => x.Timestamp)
            .ThenBy(y => y.Id)
            .Take(takeLimit)
            .ToListAsync(cancellationToken);

            return Results.Ok(res);
        });

        group.MapGet("/{id:guid}", async (Guid id, LoggyDbContext db, CancellationToken cancellation) =>
        {
            if (id == Guid.Empty)
            {
                return Results.BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = "id can't be empty" });
            }
            var query = db.Events.AsNoTracking();
            var res = await query.FirstOrDefaultAsync(e => e.Id == id, cancellation);
            return res is not null ? Results.Ok(res) : Results.NotFound(new { statusCode = StatusCodes.Status404NotFound, message = "Event not found" });
        });
    }
}