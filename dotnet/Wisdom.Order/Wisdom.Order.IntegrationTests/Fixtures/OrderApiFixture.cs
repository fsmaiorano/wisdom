using Microsoft.Extensions.DependencyInjection;
using Order.Api;
using Wisdom.Order.Infrastructure.Data;
using Wisdom.Tests.Common.Factories;
namespace Wisdom.Order.IntegrationTests.Fixtures;

public class OrderApiFixture : WebApplicationFactoryBase<OrderProgram>, IAsyncLifetime
{
    public OrderDbContext DbContext { get; private set; } = null!;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        TestDbFactory.ReplaceWithInMemory<OrderDbContext>(services);
    }

    public Task InitializeAsync()
    {
        DbContext = TestDbFactory.GetContext<OrderDbContext>(Services);
        DbContext.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        return Task.CompletedTask;
    }
}
