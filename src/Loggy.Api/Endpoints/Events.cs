namespace Loggy.Api.Endpoints.Events;

public static class EventsEndpoint
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        app.MapGet("/events", async () =>
        {
            return Results.Ok(new { message = "Hello world" });
        });
    }
}