using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("/purchase", async (PurchaseCourseRequest request, IOrderApiClient client, CancellationToken ct) =>
            {
                var result = await client.PurchaseCourseAsync(request, ct);
                return Results.Created($"/api/orders/purchase/{result.PurchaseId}", result);
            })
            .WithName("PurchaseCourse");

        group.MapGet("/enrollments", async (IOrderApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetEnrollmentsAsync(ct)))
            .WithName("GetEnrollments");

        group.MapGet("/enrollments/{courseId:guid}/check", async (Guid courseId, IOrderApiClient client, CancellationToken ct) =>
                Results.Ok(await client.CheckEnrollmentAsync(courseId, ct)))
            .WithName("CheckEnrollment");

        return app;
    }
}
