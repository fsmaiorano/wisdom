using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class UpdateCourseUseCase(ICourseRepository courseRepository)
{
    public async Task<Result<GetCourseResponse>> ExecuteAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken = default)
    {
        var getResult = await courseRepository.GetByIdAsync(CourseId.Of(id), cancellationToken);
        if (!getResult.IsSuccess)
            return Result<GetCourseResponse>.Failure(getResult.Error!);

        if (getResult.Data is null)
            return Result<GetCourseResponse>.Failure("Course not found.");

        var course = getResult.Data;
        course.Update(request.Title, request.Description, request.Price);

        var updateResult = await courseRepository.UpdateAsync(course, cancellationToken);
        if (!updateResult.IsSuccess)
            return Result<GetCourseResponse>.Failure(updateResult.Error!);

        return Result<GetCourseResponse>.Success(new GetCourseResponse(
            updateResult.Data!.Id.Value,
            updateResult.Data.TeacherId,
            updateResult.Data.Title,
            updateResult.Data.Description,
            updateResult.Data.Price,
            updateResult.Data.IsPublished,
            updateResult.Data.CreatedAt));
    }
}
