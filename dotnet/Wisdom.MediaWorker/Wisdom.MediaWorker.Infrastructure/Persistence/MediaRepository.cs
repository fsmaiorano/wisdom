using Npgsql;
using Wisdom.MediaWorker.Application.Abstractions;
using Wisdom.MediaWorker.Application.Services;
namespace Wisdom.MediaWorker.Infrastructure.Persistence;

/// <summary>Plain SQL over the "Media" table owned by Wisdom.Media (schema/migrations live there).</summary>
public class MediaRepository(NpgsqlDataSource dataSource) : IMediaRepository
{
    public async Task SetStatusAsync(Guid mediaId, string status, CancellationToken ct = default)
    {
        await using var cmd = dataSource.CreateCommand(
            """UPDATE "Media" SET "Status" = @status, "LastModifiedAt" = @now WHERE "Id" = @id""");
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("id", mediaId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task MarkReadyAsync(Guid mediaId, string thumbnailPath, int durationSeconds,
        CancellationToken ct = default)
    {
        await using var cmd = dataSource.CreateCommand(
            """
            UPDATE "Media"
            SET "Status" = @status, "ThumbnailPath" = @thumbnail, "Duration" = @duration, "LastModifiedAt" = @now
            WHERE "Id" = @id
            """);
        cmd.Parameters.AddWithValue("status", MediaStatuses.Ready);
        cmd.Parameters.AddWithValue("thumbnail", thumbnailPath);
        cmd.Parameters.AddWithValue("duration", durationSeconds);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("id", mediaId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<StaleMedia>> ClaimStaleAsync(DateTime cutoff, CancellationToken ct = default)
    {
        await using var cmd = dataSource.CreateCommand(
            """
            UPDATE "Media"
            SET    "Status" = 'Processing',
                   "LastModifiedAt" = NOW()
            WHERE  "Status" IN ('Uploaded', 'Failed')
              AND  "LastModifiedAt" < @cutoff
            RETURNING "Id", "LessonId", "Type", "FilePath"
            """);
        cmd.Parameters.AddWithValue("cutoff", DateTime.SpecifyKind(cutoff, DateTimeKind.Utc));

        var claimed = new List<StaleMedia>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            claimed.Add(new StaleMedia(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2), reader.GetString(3)));

        return claimed;
    }
}
