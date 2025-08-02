using Focuswave.SessionTrackingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Focuswave.SessionTrackingService.Persistence.Configuration;

public class FocusCycleConfiguration : IEntityTypeConfiguration<FocusCycle>
{
    public void Configure(EntityTypeBuilder<FocusCycle> builder)
    {
        builder.ToTable("FocusCycles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.EndedAt).IsRequired(false);

        builder.HasIndex(x => x.UserId).IsUnique(false);
    }
}
