using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Application.UseCases;

public class DeleteLessonUseCase(ILessonRepository lessonRepository)
{
    public async Task<Result<bool>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleteResult = await lessonRepository.DeleteAsync(LessonId.Of(id), cancellationToken);
        if (!deleteResult.IsSuccess)
            return Result<bool>.Failure(deleteResult.Error!);

        return Result<bool>.Success(deleteResult.Data);
    }
}
