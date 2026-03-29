namespace Wisdom.Bff.Aggregations.Dashboard;

public static class DashboardEndpoint
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/bff/dashboard", async (DashboardHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.ExecuteAsync(ct)))
        .WithName("GetDashboard")
        .WithSummary("Student dashboard — enrolled courses with aggregated progress in one call")
        .WithTags("BFF — Aggregations")
        .RequireAuthorization()
        .Produces<DashboardResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
