using Loggy.Api.Endpoints.Events;
using Loggy.Api.ErrorHandling;
using Loggy.Core.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext(builder.Configuration.GetConnectionString("LoggyDb") ?? "Data Source=loggy.db");
builder.Services.AddValidation();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler(_ => { }); // Smother the default with a pillow and make it bubble to GlobalExceptionHandler
app.Services.EnsureDbCreated();
app.MapEventEndpoints();

app.Run();
