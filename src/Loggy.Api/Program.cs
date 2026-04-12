using Loggy.Api.Endpoints.Events;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapEventEndpoints();

app.Run();
