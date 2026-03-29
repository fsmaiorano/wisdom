using Microsoft.EntityFrameworkCore;
using Wisdom.BuildingBlocks;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.Repositories;
using Wisdom.Order.Infrastructure.Data;
namespace Wisdom.Order.Infrastructure.Repositories;

public class EnrollmentRepository(OrderDbContext dbContext) : IEnrollmentRepository
{
    public async Task<Result<IReadOnlyList<EnrollmentEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var enrollments = await dbContext.Enrollments
                .Where(e => e.UserId == userId)
                .ToListAsync(ct);
            return Result<IReadOnlyList<EnrollmentEntity>>.Success(enrollments);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<EnrollmentEntity>>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> IsEnrolledAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        try
        {
            var enrolled = await dbContext.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, ct);
            return Result<bool>.Success(enrolled);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<EnrollmentEntity>> CreateAsync(EnrollmentEntity enrollment, CancellationToken ct = default)
    {
        try
        {
            await dbContext.Enrollments.AddAsync(enrollment, ct);
            await dbContext.SaveChangesAsync(ct);
            return Result<EnrollmentEntity>.Success(enrollment);
        }
        catch (Exception ex)
        {
            return Result<EnrollmentEntity>.Failure(ex.Message);
        }
    }
}
