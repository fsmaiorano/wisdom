using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
using Wisdom.Course.Infrastructure.Data;
namespace Wisdom.Course.Infrastructure.Repositories;

public class LessonRepository(CourseDbContext dbContext) : ILessonRepository
{
    public async Task<Result<LessonEntity?>> GetByIdAsync(LessonId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var lesson = await dbContext.Lessons
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
            return Result<LessonEntity?>.Success(lesson);
        }
        catch (Exception ex)
        {
            return Result<LessonEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<LessonEntity>>> GetByCourseIdAsync(CourseId courseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var lessons = await dbContext.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Position)
                .ToListAsync(cancellationToken);
            return Result<IReadOnlyList<LessonEntity>>.Success(lessons);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<LessonEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<LessonEntity>> CreateAsync(LessonEntity lesson, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Lessons.AddAsync(lesson, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<LessonEntity>.Success(lesson);
        }
        catch (Exception ex)
        {
            return Result<LessonEntity>.Failure(ex.Message);
        }
    }

    public async Task<Result<LessonEntity>> UpdateAsync(LessonEntity lesson, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.Lessons.Update(lesson);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<LessonEntity>.Success(lesson);
        }
        catch (Exception ex)
        {
            return Result<LessonEntity>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(LessonId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var lesson = await dbContext.Lessons
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

            if (lesson is null)
                return Result<bool>.Failure("Lesson not found.");

            dbContext.Lessons.Remove(lesson);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
