using Wisdom.BuildingBlocks;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Repositories;
namespace Wisdom.User.UnitTests.Mocks;

public class FakeUserRepository : IUserRepository
{
    private readonly List<UserEntity> _store = [];

    public bool GetByKeycloakIdShouldFail { get; set; }
    public bool GetByEmailShouldFail      { get; set; }
    public bool ExistsShouldFail          { get; set; }
    public bool CreateShouldFail          { get; set; }

    public int GetByKeycloakIdCallCount { get; private set; }
    public int GetByEmailCallCount      { get; private set; }
    public int ExistsCallCount          { get; private set; }
    public int CreateCallCount          { get; private set; }

    public string?     LastKeycloakId   { get; private set; }
    public string?     LastEmail        { get; private set; }
    public UserEntity? LastCreatedUser  { get; private set; }

    public FakeUserRepository Seed(params UserEntity[] users)
    {
        _store.AddRange(users);
        return this;
    }

    public IReadOnlyList<UserEntity> All => _store.AsReadOnly();

    public Task<Result<UserEntity?>> GetByKeycloakIdAsync(
        string keycloakId, CancellationToken cancellationToken = default)
    {
        GetByKeycloakIdCallCount++;
        LastKeycloakId = keycloakId;

        if (GetByKeycloakIdShouldFail)
            return Task.FromResult(Result<UserEntity?>.Failure("Repository error: GetByKeycloakId failed."));

        var user = _store.FirstOrDefault(u => u.KeycloakId == keycloakId);
        return Task.FromResult(Result<UserEntity?>.Success(user));
    }

    public Task<Result<UserEntity?>> GetByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        GetByEmailCallCount++;
        LastEmail = email;

        if (GetByEmailShouldFail)
            return Task.FromResult(Result<UserEntity?>.Failure("Repository error: GetByEmail failed."));

        var user = _store.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(Result<UserEntity?>.Success(user));
    }

    public Task<Result<bool>> ExistsAsync(
        string email, CancellationToken cancellationToken = default)
    {
        ExistsCallCount++;

        if (ExistsShouldFail)
            return Task.FromResult(Result<bool>.Failure("Repository error: Exists failed."));

        var exists = _store.Any(u => u.Email == email);
        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result<UserEntity>> CreateAsync(
        UserEntity user, CancellationToken cancellationToken = default)
    {
        CreateCallCount++;
        LastCreatedUser = user;

        if (CreateShouldFail)
            return Task.FromResult(Result<UserEntity>.Failure("Repository error: Create failed."));

        _store.Add(user);
        return Task.FromResult(Result<UserEntity>.Success(user));
    }
}
