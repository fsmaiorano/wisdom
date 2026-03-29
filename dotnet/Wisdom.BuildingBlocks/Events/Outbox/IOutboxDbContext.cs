using Microsoft.EntityFrameworkCore;
namespace Wisdom.BuildingBlocks.Events.Outbox;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
}

