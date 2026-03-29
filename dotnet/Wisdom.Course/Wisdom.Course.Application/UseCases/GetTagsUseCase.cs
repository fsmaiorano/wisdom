using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Repositories;
namespace Wisdom.Course.Application.UseCases;

public class GetTagsUseCase(ITagRepository tagRepository)
{
    public async Task<Result<IReadOnlyList<GetTagResponse>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var getResult = await tagRepository.GetAllAsync(cancellationToken);
        if (!getResult.IsSuccess)
            return Result<IReadOnlyList<GetTagResponse>>.Failure(getResult.Error!);

        var response = getResult.Data!
            .Select(t => new GetTagResponse(t.Id.Value, t.Name))
            .ToList();

        return Result<IReadOnlyList<GetTagResponse>>.Success(response);
    }
}
