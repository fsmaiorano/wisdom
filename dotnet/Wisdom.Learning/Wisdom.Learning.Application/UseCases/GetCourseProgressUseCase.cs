using Wisdom.BuildingBlocks;
using Wisdom.Learning.Application.Dtos;
using Wisdom.Learning.Domain.Repositories;
namespace Wisdom.Learning.Application.UseCases;

public class GetCourseProgressUseCase(IProgressRepository progressRepository)
{
    public async Task<Result<List<GetProgressResponse>>> ExecuteAsync(Guid userId, GetCourseProgressRequest request, CancellationToken ct = default)
    {
        var result = await progressRepository.GetByUserAndLessonIdsAsync(userId, request.LessonIds, ct);
        if (!result.IsSuccess)
            return Result<List<GetProgressResponse>>.Failure(result.Error!);

        var responses = result.Data!
            .Select(p => new GetProgressResponse(
                p.Id.Value,
                p.UserId,
                p.LessonId,
                p.WatchedSeconds,
                p.IsCompleted,
                p.LastModifiedAt))
            .ToList();

        return Result<List<GetProgressResponse>>.Success(responses);
    }
}
