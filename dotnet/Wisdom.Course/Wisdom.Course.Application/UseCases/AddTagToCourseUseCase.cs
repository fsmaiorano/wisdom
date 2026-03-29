using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Repositories;
namespace Wisdom.Course.Application.UseCases;

public class AddTagToCourseUseCase(ITagRepository tagRepository)
{
    public async Task<Result<bool>> ExecuteAsync(Guid courseId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var addResult = await tagRepository.AddTagToCourseAsync(courseId, tagId, cancellationToken);
        if (!addResult.IsSuccess)
            return Result<bool>.Failure(addResult.Error!);

        return Result<bool>.Success(addResult.Data);
    }
}
