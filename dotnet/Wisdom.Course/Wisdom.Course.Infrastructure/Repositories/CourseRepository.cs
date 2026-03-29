using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
using Wisdom.Course.Infrastructure.Data;
namespace Wisdom.Course.Infrastructure.Repositories;

public class CourseRepository(CourseDbContext dbContext) : ICourseRepository
{
    public async Task<Result<CourseEntity?>> GetByIdAsync(CourseId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var course = await dbContext.Courses
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            return Result<CourseEntity?>.Success(course);
        }
        catch (Exception ex)
        {
            return Result<CourseEntity?>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<CourseEntity>>> GetAllPublishedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var courses = await dbContext.Courses
                .Where(c => c.IsPublished)
                .ToListAsync(cancellationToken);
            return Result<IReadOnlyList<CourseEntity>>.Success(courses);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CourseEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<IReadOnlyList<CourseEntity>>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        try
        {
            var courses = await dbContext.Courses
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync(cancellationToken);
            return Result<IReadOnlyList<CourseEntity>>.Success(courses);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CourseEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<CourseEntity>> CreateAsync(CourseEntity course, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Courses.AddAsync(course, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<CourseEntity>.Success(course);
        }
        catch (Exception ex)
        {
            return Result<CourseEntity>.Failure(ex.Message);
        }
    }

    public async Task<Result<CourseEntity>> UpdateAsync(CourseEntity course, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.Courses.Update(course);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<CourseEntity>.Success(course);
        }
        catch (Exception ex)
        {
            return Result<CourseEntity>.Failure(ex.Message);
        }
    }
}
