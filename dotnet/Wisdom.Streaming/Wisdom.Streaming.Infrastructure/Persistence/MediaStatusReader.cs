using Npgsql;
using Wisdom.Streaming.Application.Abstractions;
namespace Wisdom.Streaming.Infrastructure.Persistence;

/// <summary>Read-only access to the "Media" table owned by Wisdom.Media.</summary>
public class MediaStatusReader(NpgsqlDataSource dataSource) : IMediaStatusReader
{
    public async Task<string?> GetStatusAsync(Guid mediaId, CancellationToken ct = default)
    {
        await using var cmd = dataSource.CreateCommand("""SELECT "Status" FROM "Media" WHERE "Id" = @id""");
        cmd.Parameters.AddWithValue("id", mediaId);
        return await cmd.ExecuteScalarAsync(ct) as string;
    }
}
