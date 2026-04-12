using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Loggy.Core.Db;

public static class Extensions
{
    public static IServiceCollection AddDbContext(this IServiceCollection serviceCollection, string connectionString)
    {
        return serviceCollection.AddDbContext<LoggyDbContext>(options => options.UseSqlite(connectionString));
    }

    public static void EnsureDbCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LoggyDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

