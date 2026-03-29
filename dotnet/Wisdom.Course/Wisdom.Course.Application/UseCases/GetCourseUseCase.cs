using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class GetCourseUseCase(ICourseRepository courseRepository)
{
    public async Task<Result<GetCourseResponse?>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var getResult = await courseRepository.GetByIdAsync(CourseId.Of(id), cancellationToken);
        if (!getResult.IsSuccess)
            return Result<GetCourseResponse?>.Failure(getResult.Error!);

        if (getResult.Data is null)
            return Result<GetCourseResponse?>.Success(null);

        var course = getResult.Data;
        return Result<GetCourseResponse?>.Success(new GetCourseResponse(
            course.Id.Value,
            course.TeacherId,
            course.Title,
            course.Description,
            course.Price,
            course.IsPublished,
            course.CreatedAt));
    }
}
