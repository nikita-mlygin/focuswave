using System.Reflection;
using Focuswave.SessionTrackingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Focuswave.SessionTrackingService.Persistence;

public class SessionTrackingDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<FocusCycle> Sessions { get; set; }
    public DbSet<FocusCycleSegment> SessionEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
