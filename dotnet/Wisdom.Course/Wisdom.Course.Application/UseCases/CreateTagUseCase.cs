using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
namespace Wisdom.Course.Application.UseCases;

public class CreateTagUseCase(ITagRepository tagRepository)
{
    public async Task<Result<GetTagResponse>> ExecuteAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        var tag = TagEntity.Create(request.Name);

        var createResult = await tagRepository.CreateAsync(tag, cancellationToken);
        if (!createResult.IsSuccess)
            return Result<GetTagResponse>.Failure(createResult.Error!);

        return Result<GetTagResponse>.Success(new GetTagResponse(
            createResult.Data!.Id.Value,
            createResult.Data.Name));
    }
}
