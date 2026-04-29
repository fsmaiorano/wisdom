using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class GetLessonUseCase(ILessonRepository lessonRepository)
{
    public async Task<Result<GetLessonResponse>> ExecuteAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var result = await lessonRepository.GetByIdAsync(LessonId.Of(lessonId), cancellationToken);
        if (!result.IsSuccess)
            return Result<GetLessonResponse>.Failure(result.Error!);

        if (result.Data is null)
            return Result<GetLessonResponse>.Failure("Lesson not found.");

        var l = result.Data;
        return Result<GetLessonResponse>.Success(new GetLessonResponse(
            l.Id.Value,
            l.CourseId.Value,
            l.Title,
            l.Description,
            l.Position,
            l.IsPreview,
            l.CreatedAt));
    }
}
