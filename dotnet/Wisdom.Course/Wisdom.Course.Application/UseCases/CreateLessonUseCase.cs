using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class CreateLessonUseCase(ILessonRepository lessonRepository)
{
    public async Task<Result<GetLessonResponse>> ExecuteAsync(Guid courseId, CreateLessonRequest request, CancellationToken cancellationToken = default)
    {
        var lesson = LessonEntity.Create(
            CourseId.Of(courseId),
            request.Title,
            request.Description,
            request.Position,
            request.IsPreview);

        var createResult = await lessonRepository.CreateAsync(lesson, cancellationToken);
        if (!createResult.IsSuccess)
            return Result<GetLessonResponse>.Failure(createResult.Error!);

        return Result<GetLessonResponse>.Success(new GetLessonResponse(
            createResult.Data!.Id.Value,
            createResult.Data.CourseId.Value,
            createResult.Data.Title,
            createResult.Data.Description,
            createResult.Data.Position,
            createResult.Data.IsPreview,
            createResult.Data.CreatedAt));
    }
}
