var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("loggy");
var sqlite = builder.AddSqlite("loggy-db");

builder.AddProject<Projects.Loggy_Api>("api")
    .WithEnvironment("ConnectionStrings__Loggy", "Data Source=/tmp/loggy.db")
    .WithReference(sqlite)
    .WaitFor(sqlite);

builder.Build().Run();
