using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
namespace Wisdom.Course.Application.UseCases;

public class ListCoursesUseCase(ICourseRepository courseRepository)
{
    public async Task<Result<IReadOnlyList<GetCourseResponse>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var getResult = await courseRepository.GetAllPublishedAsync(cancellationToken);
        if (!getResult.IsSuccess)
            return Result<IReadOnlyList<GetCourseResponse>>.Failure(getResult.Error!);

        var response = getResult.Data!
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
