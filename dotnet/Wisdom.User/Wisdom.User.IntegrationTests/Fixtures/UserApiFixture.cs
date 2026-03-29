using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using User.Api;
using Wisdom.Tests.Common.Factories;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Infrastructure.Data;
using Wisdom.User.IntegrationTests.Mocks;
namespace Wisdom.User.IntegrationTests.Fixtures;

public class UserApiFixture : WebApplicationFactoryBase<UserProgram>, IAsyncLifetime
{
    public TestKeycloakService KeycloakService { get; } = new();
    public UserDbContext DbContext { get; private set; } = null!;

    protected override void ConfigureTestServices(IServiceCollection services)
    {

        TestDbFactory.ReplaceWithInMemory<UserDbContext>(services);

        services.RemoveAll<IKeycloakService>();
        services.AddSingleton<IKeycloakService>(KeycloakService);
    }

    public Task InitializeAsync()
    {
        DbContext = TestDbFactory.GetContext<UserDbContext>(Services);
        DbContext.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        return Task.CompletedTask;
    }
}
