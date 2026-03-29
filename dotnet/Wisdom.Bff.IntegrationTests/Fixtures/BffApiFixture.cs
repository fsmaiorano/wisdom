using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wisdom.Bff;
using Wisdom.Bff.Clients;
using Wisdom.Bff.IntegrationTests.Infrastructure;
using Wisdom.Tests.Common.Factories;

namespace Wisdom.Bff.IntegrationTests.Fixtures;

public sealed class BffApiFixture : WebApplicationFactoryBase<BffProgram>
{
    public readonly TestHttpMessageHandler CourseBackend = new();
    public readonly TestHttpMessageHandler OrderBackend = new();
    public readonly TestHttpMessageHandler LearningBackend = new();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Replace the typed HTTP clients with test-controlled message handlers.
        // The ICourseApiClient → CourseApiClient registration is preserved,
        // so handlers still receive the real typed client backed by our stub.

        services.RemoveAll<CourseApiClient>();
        services.AddHttpClient<CourseApiClient>(c =>
            c.BaseAddress = new Uri("http://course-test"))
            .ConfigurePrimaryHttpMessageHandler(() => CourseBackend);

        services.RemoveAll<OrderApiClient>();
        services.AddHttpClient<OrderApiClient>(c =>
            c.BaseAddress = new Uri("http://order-test"))
            .ConfigurePrimaryHttpMessageHandler(() => OrderBackend);

        services.RemoveAll<LearningApiClient>();
        services.AddHttpClient<LearningApiClient>(c =>
            c.BaseAddress = new Uri("http://learning-test"))
            .ConfigurePrimaryHttpMessageHandler(() => LearningBackend);
    }
}
