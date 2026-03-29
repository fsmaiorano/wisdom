using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
namespace Wisdom.Course.Application.UseCases;

public class CreateCourseUseCase(ICourseRepository courseRepository)
{
    public async Task<Result<CreateCourseResponse>> ExecuteAsync(Guid teacherId, CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        var course = CourseEntity.Create(teacherId, request.Title, request.Description, request.Price);

        var createResult = await courseRepository.CreateAsync(course, cancellationToken);
        if (!createResult.IsSuccess)
            return Result<CreateCourseResponse>.Failure(createResult.Error!);

        return Result<CreateCourseResponse>.Success(new CreateCourseResponse(
            createResult.Data!.Id.Value,
            createResult.Data.TeacherId,
            createResult.Data.Title,
            createResult.Data.Description,
            createResult.Data.Price,
            createResult.Data.IsPublished,
            createResult.Data.CreatedAt));
    }
}
