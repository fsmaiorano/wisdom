using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.Domain.Repositories;
using Wisdom.Learning.Domain.ValueObjects;
using Wisdom.Learning.Infrastructure.Data;
namespace Wisdom.Learning.Infrastructure.Repositories;

public class ProgressRepository(LearningDbContext dbContext) : IProgressRepository
{
    public async Task<Result<ProgressEntity?>> GetByIdAsync(ProgressId id, CancellationToken ct = default)
    {
        try
        {
            var progress = await dbContext.Progress.FirstOrDefaultAsync(p => p.Id == id, ct);
            return Result<ProgressEntity?>.Success(progress);
        }
        catch (Exception ex)
        {
            return Result<ProgressEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<ProgressEntity?>> GetByUserAndLessonAsync(Guid userId, Guid lessonId, CancellationToken ct = default)
    {
        try
        {
            var progress = await dbContext.Progress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);
            return Result<ProgressEntity?>.Success(progress);
        }
        catch (Exception ex)
        {
            return Result<ProgressEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<ProgressEntity>>> GetByUserAndLessonIdsAsync(Guid userId, IEnumerable<Guid> lessonIds, CancellationToken ct = default)
    {
        try
        {
            var ids = lessonIds.ToList();
            var progressList = await dbContext.Progress
                .Where(p => p.UserId == userId && ids.Contains(p.LessonId))
                .ToListAsync(ct);
            return Result<IReadOnlyList<ProgressEntity>>.Success(progressList.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ProgressEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<ProgressEntity>> CreateAsync(ProgressEntity progress, CancellationToken ct = default)
    {
        try
        {
            await dbContext.Progress.AddAsync(progress, ct);
            await dbContext.SaveChangesAsync(ct);
            return Result<ProgressEntity>.Success(progress);
        }
        catch (Exception ex)
        {
            return Result<ProgressEntity>.Failure(ex.Message);
        }
    }

    public async Task<Result<ProgressEntity>> UpdateAsync(ProgressEntity progress, CancellationToken ct = default)
    {
        try
        {
            dbContext.Progress.Update(progress);
            await dbContext.SaveChangesAsync(ct);
            return Result<ProgressEntity>.Success(progress);
        }
        catch (Exception ex)
        {
            return Result<ProgressEntity>.Failure(ex.Message);
        }
    }
}
