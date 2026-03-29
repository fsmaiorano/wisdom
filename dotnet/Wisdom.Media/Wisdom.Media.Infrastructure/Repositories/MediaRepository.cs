using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Domain.ValueObjects;
using Wisdom.Media.Infrastructure.Data;
namespace Wisdom.Media.Infrastructure.Repositories;

public class MediaRepository(MediaDbContext dbContext) : IMediaRepository
{
    public async Task<Result<MediaEntity?>> GetByIdAsync(MediaId id, CancellationToken ct = default)
    {
        try
        {
            var media = await dbContext.Media.FirstOrDefaultAsync(m => m.Id == id, ct);
            return Result<MediaEntity?>.Success(media);
        }
        catch (Exception ex)
        {
            return Result<MediaEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<MediaEntity>>> GetByLessonIdAsync(Guid lessonId, CancellationToken ct = default)
    {
        try
        {
            var media = await dbContext.Media
                .Where(m => m.LessonId == lessonId)
                .ToListAsync(ct);
            return Result<IReadOnlyList<MediaEntity>>.Success(media.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MediaEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<MediaEntity>> CreateAsync(MediaEntity media, CancellationToken ct = default)
    {
        try
        {
            await dbContext.Media.AddAsync(media, ct);
            await dbContext.SaveChangesAsync(ct);
            return Result<MediaEntity>.Success(media);
        }
        catch (Exception ex)
        {
            return Result<MediaEntity>.Failure(ex.Message);
        }
    }

    public async Task<Result<MediaEntity>> UpdateAsync(MediaEntity media, CancellationToken ct = default)
    {
        try
        {
            dbContext.Media.Update(media);
            await dbContext.SaveChangesAsync(ct);
            return Result<MediaEntity>.Success(media);
        }
        catch (Exception ex)
        {
            return Result<MediaEntity>.Failure(ex.Message);
        }
    }
}
