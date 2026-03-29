using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wisdom.BuildingBlocks.Events.Outbox;

public class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RetryCount).HasDefaultValue(0);
        builder.Property(x => x.ScheduledAfter).IsRequired(false);

        // Index to speed up the processor's polling query.
        builder.HasIndex(x => x.ProcessedOn);
        builder.HasIndex(x => x.ScheduledAfter);
    }
}
