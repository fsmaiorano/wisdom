using System.Text.Json;

namespace Wisdom.Bff.Clients;

public sealed class LearningApiClient(HttpClient httpClient) : ILearningApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<BackendProgress>> GetCourseProgressAsync(
        IReadOnlyList<Guid> lessonIds, CancellationToken ct)
    {
        if (lessonIds.Count == 0)
            return [];

        var response = await httpClient.PostAsJsonAsync(
            "/api/progress/course", new { lessonIds }, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<BackendProgress>>(JsonOptions, ct) ?? [];
    }
}
