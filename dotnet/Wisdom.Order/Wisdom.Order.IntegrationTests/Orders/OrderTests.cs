using System.Net;
using System.Net.Http.Json;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
using Wisdom.Order.Application.Dtos;
using Wisdom.Order.IntegrationTests.Fixtures;
namespace Wisdom.Order.IntegrationTests.Orders;

public class OrderTests(OrderApiFixture fixture) : IClassFixture<OrderApiFixture>
{
    private static string NewUserToken() => AuthHelper.TokenForEmail(BogusFactory.Email());

    [Fact]
    public async Task PurchaseCourse_WithValidRequest_Returns201WithPurchaseData()
    {
        var token = NewUserToken();
        var courseId = BogusFactory.NewGuid();
        var price = BogusFactory.Faker.Random.Decimal(1, 999);
        var request = new PurchaseCourseRequest(courseId, price);

        var response = await fixture.DoPost("/api/orders/purchase", request, token);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PurchaseCourseResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.PurchaseId);
        Assert.Equal(courseId, body.CourseId);
        Assert.Equal(price, body.Price);
    }

    [Fact]
    public async Task PurchaseCourse_WithoutAuth_Returns401()
    {
        var request = new PurchaseCourseRequest(BogusFactory.NewGuid(), BogusFactory.Faker.Random.Decimal(1, 500));

        var response = await fixture.DoPost("/api/orders/purchase", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PurchaseCourse_ThenCheckEnrollment_IsEnrolledTrue()
    {
        var token = NewUserToken();
        var courseId = BogusFactory.NewGuid();
        var price = BogusFactory.Faker.Random.Decimal(1, 500);

        var purchaseResponse = await fixture.DoPost(
            "/api/orders/purchase", new PurchaseCourseRequest(courseId, price), token);
        Assert.Equal(HttpStatusCode.Created, purchaseResponse.StatusCode);

        var checkResponse = await fixture.DoGet($"/api/orders/enrollments/{courseId}/check", token);
        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);

        var enrollment = await checkResponse.Content.ReadFromJsonAsync<CheckEnrollmentResponse>();
        Assert.NotNull(enrollment);
        Assert.True(enrollment.IsEnrolled);
    }

    [Fact]
    public async Task GetEnrollments_AfterPurchase_ReturnsEnrollmentList()
    {
        var token = NewUserToken();
        var courseId = BogusFactory.NewGuid();

        await fixture.DoPost("/api/orders/purchase",
            new PurchaseCourseRequest(courseId, BogusFactory.Faker.Random.Decimal(1, 500)), token);

        var response = await fixture.DoGet("/api/orders/enrollments", token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<GetEnrollmentResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body, e => e.CourseId == courseId);
    }

    [Fact]
    public async Task CheckEnrollment_WithoutPurchase_IsEnrolledFalse()
    {
        var token = NewUserToken();
        var courseId = BogusFactory.NewGuid();

        var response = await fixture.DoGet($"/api/orders/enrollments/{courseId}/check", token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CheckEnrollmentResponse>();
        Assert.NotNull(body);
        Assert.False(body.IsEnrolled);
    }
}
