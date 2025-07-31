using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Focuswave.SessionTrackingService.Persistence.Configuration;

public class FocusCycleSegmentConfiguration : IEntityTypeConfiguration<FocusCycleSegment>
{
    public void Configure(EntityTypeBuilder<FocusCycleSegment> builder)
    {
        builder.ToTable("FocusCycleSegments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CycleId).IsRequired();
        builder
            .HasOne<FocusCycle>()
            .WithMany()
            .HasForeignKey(x => x.CycleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.EndedAt).IsRequired(false);

        builder.Property(x => x.Type).IsRequired();

        builder.Property(x => x.FocusSessionId).IsRequired(false);

        builder.Property(x => x.PlannedDuration).IsRequired(false);
        builder.Property(x => x.ActualDuration).IsRequired(false);

        builder.HasIndex(x => new { x.CycleId, x.StartedAt });
        builder.HasIndex(x => x.FocusSessionId);
    }
}
