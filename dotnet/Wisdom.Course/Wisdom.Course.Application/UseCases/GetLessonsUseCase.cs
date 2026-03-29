using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class GetLessonsUseCase(ILessonRepository lessonRepository)
{
    public async Task<Result<IReadOnlyList<GetLessonResponse>>> ExecuteAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var getResult = await lessonRepository.GetByCourseIdAsync(CourseId.Of(courseId), cancellationToken);
        if (!getResult.IsSuccess)
            return Result<IReadOnlyList<GetLessonResponse>>.Failure(getResult.Error!);

        var response = getResult.Data!
            .Select(l => new GetLessonResponse(
                l.Id.Value,
                l.CourseId.Value,
                l.Title,
                l.Description,
                l.Position,
                l.IsPreview,
                l.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<GetLessonResponse>>.Success(response);
    }
}
