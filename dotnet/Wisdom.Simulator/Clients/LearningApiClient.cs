using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

public class LearningApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    public Task<GetProgressResponse?> UpdateProgressAsync(UpdateProgressRequest request, CancellationToken ct = default)
        => PostAsync<GetProgressResponse>("/api/progress", request, ct);

    public Task<GetProgressResponse?> GetProgressAsync(Guid lessonId, CancellationToken ct = default)
        => GetAsync<GetProgressResponse>($"/api/progress/lesson/{lessonId}", ct);
}
