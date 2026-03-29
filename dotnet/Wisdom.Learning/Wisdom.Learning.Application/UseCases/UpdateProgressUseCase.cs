using Wisdom.BuildingBlocks;
using Wisdom.Learning.Application.Dtos;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.Domain.Repositories;
namespace Wisdom.Learning.Application.UseCases;

public class UpdateProgressUseCase(IProgressRepository progressRepository)
{
    public async Task<Result<GetProgressResponse>> ExecuteAsync(Guid userId, UpdateProgressRequest request, CancellationToken ct = default)
    {
        var existingResult = await progressRepository.GetByUserAndLessonAsync(userId, request.LessonId, ct);
        if (!existingResult.IsSuccess)
            return Result<GetProgressResponse>.Failure(existingResult.Error!);

        ProgressEntity progress;

        if (existingResult.Data is null)
        {
            progress = ProgressEntity.Create(userId, request.LessonId);
            progress.Update(request.WatchedSeconds, request.IsCompleted);

            var createResult = await progressRepository.CreateAsync(progress, ct);
            if (!createResult.IsSuccess)
                return Result<GetProgressResponse>.Failure(createResult.Error!);

            progress = createResult.Data!;
        }
        else
        {
            progress = existingResult.Data;
            progress.Update(request.WatchedSeconds, request.IsCompleted);

            var updateResult = await progressRepository.UpdateAsync(progress, ct);
            if (!updateResult.IsSuccess)
                return Result<GetProgressResponse>.Failure(updateResult.Error!);

            progress = updateResult.Data!;
        }

        return Result<GetProgressResponse>.Success(new GetProgressResponse(
            progress.Id.Value,
            progress.UserId,
            progress.LessonId,
            progress.WatchedSeconds,
            progress.IsCompleted,
            progress.LastModifiedAt));
    }
}
