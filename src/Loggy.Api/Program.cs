using Loggy.Api.Endpoints.Events;
using Loggy.Core.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext(builder.Configuration.GetConnectionString("LoggyDb") ?? "Data Source=loggy.db");
builder.Services.AddValidation();

var app = builder.Build();
app.Services.EnsureDbCreated();
app.MapEventEndpoints();

app.Run();
