namespace Loggy.Core.Db;

using Microsoft.EntityFrameworkCore;
using Models.Events;

public class LoggyDbContext(DbContextOptions<LoggyDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);
        var entity = modelBuilder.Entity<Event>();
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Amount).HasPrecision(18, 4);
        entity.Property(e => e.Type).IsRequired();
        entity.HasIndex(e => e.Timestamp);
    }
}
