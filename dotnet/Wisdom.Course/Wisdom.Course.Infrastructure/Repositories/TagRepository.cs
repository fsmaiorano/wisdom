using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
using Wisdom.Course.Infrastructure.Data;
namespace Wisdom.Course.Infrastructure.Repositories;

public class TagRepository(CourseDbContext dbContext) : ITagRepository
{
    public async Task<Result<IReadOnlyList<TagEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await dbContext.Tags.ToListAsync(cancellationToken);
            return Result<IReadOnlyList<TagEntity>>.Success(tags);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TagEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<TagEntity?>> GetByIdAsync(TagId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tag = await dbContext.Tags
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            return Result<TagEntity?>.Success(tag);
        }
        catch (Exception ex)
        {
            return Result<TagEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<TagEntity>> CreateAsync(TagEntity tag, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Tags.AddAsync(tag, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<TagEntity>.Success(tag);
        }
        catch (Exception ex)
        {
            return Result<TagEntity>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> AddTagToCourseAsync(Guid courseId, Guid tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            var courseTag = new CourseTagEntity { CourseId = courseId, TagId = tagId };
            await dbContext.CourseTags.AddAsync(courseTag, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<TagEntity>>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tagIds = await dbContext.CourseTags
                .Where(ct => ct.CourseId == courseId)
                .Select(ct => ct.TagId)
                .ToListAsync(cancellationToken);

            var tags = await dbContext.Tags
                .Where(t => tagIds.Contains(t.Id.Value))
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<TagEntity>>.Success(tags);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TagEntity>>.Failure(ex.Message);
        }
    }
}
