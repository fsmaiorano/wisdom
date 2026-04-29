using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
namespace Wisdom.Course.Application.UseCases;

public class GetTeacherCoursesUseCase(ICourseRepository courseRepository)
{
    public async Task<Result<IReadOnlyList<GetCourseResponse>>> ExecuteAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        var result = await courseRepository.GetByTeacherIdAsync(teacherId, cancellationToken);
        if (!result.IsSuccess)
            return Result<IReadOnlyList<GetCourseResponse>>.Failure(result.Error!);

        var response = result.Data!
            .Select(c => new GetCourseResponse(
                c.Id.Value,
                c.TeacherId,
                c.Title,
                c.Description,
                c.Price,
                c.IsPublished,
                c.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<GetCourseResponse>>.Success(response);
    }
}
