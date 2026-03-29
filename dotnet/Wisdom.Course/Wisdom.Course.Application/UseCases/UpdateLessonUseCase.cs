using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class UpdateLessonUseCase(ILessonRepository lessonRepository)
{
    public async Task<Result<GetLessonResponse>> ExecuteAsync(Guid id, UpdateLessonRequest request, CancellationToken cancellationToken = default)
    {
        var getResult = await lessonRepository.GetByIdAsync(LessonId.Of(id), cancellationToken);
        if (!getResult.IsSuccess)
            return Result<GetLessonResponse>.Failure(getResult.Error!);

        if (getResult.Data is null)
            return Result<GetLessonResponse>.Failure("Lesson not found.");

        var lesson = getResult.Data;
        lesson.Update(request.Title, request.Description, request.Position, request.IsPreview);

        var updateResult = await lessonRepository.UpdateAsync(lesson, cancellationToken);
        if (!updateResult.IsSuccess)
            return Result<GetLessonResponse>.Failure(updateResult.Error!);

        return Result<GetLessonResponse>.Success(new GetLessonResponse(
            updateResult.Data!.Id.Value,
            updateResult.Data.CourseId.Value,
            updateResult.Data.Title,
            updateResult.Data.Description,
            updateResult.Data.Position,
            updateResult.Data.IsPreview,
            updateResult.Data.CreatedAt));
    }
}
