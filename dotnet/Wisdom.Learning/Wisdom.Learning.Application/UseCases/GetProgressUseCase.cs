using Wisdom.BuildingBlocks;
using Wisdom.Learning.Application.Dtos;
using Wisdom.Learning.Domain.Repositories;
namespace Wisdom.Learning.Application.UseCases;

public class GetProgressUseCase(IProgressRepository progressRepository)
{
    public async Task<Result<GetProgressResponse>> ExecuteAsync(Guid userId, Guid lessonId, CancellationToken ct = default)
    {
        var result = await progressRepository.GetByUserAndLessonAsync(userId, lessonId, ct);
        if (!result.IsSuccess)
            return Result<GetProgressResponse>.Failure(result.Error!);

        if (result.Data is null)
        {
            return Result<GetProgressResponse>.Success(new GetProgressResponse(
                Guid.Empty,
                userId,
                lessonId,
                0,
                false,
                null));
        }

        var progress = result.Data;
        return Result<GetProgressResponse>.Success(new GetProgressResponse(
            progress.Id.Value,
            progress.UserId,
            progress.LessonId,
            progress.WatchedSeconds,
            progress.IsCompleted,
            progress.LastModifiedAt));
    }
}
