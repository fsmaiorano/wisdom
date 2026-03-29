using System.Net;
using System.Text.Json;

namespace Wisdom.Bff.Clients;

public sealed class CourseApiClient(HttpClient httpClient) : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<BackendCourse?> GetCourseAsync(Guid courseId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync($"/api/courses/{courseId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BackendCourse>(JsonOptions, ct);
    }

    public async Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync($"/api/courses/{courseId}/lessons", ct);

        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<BackendLesson>>(JsonOptions, ct) ?? [];
    }
}
