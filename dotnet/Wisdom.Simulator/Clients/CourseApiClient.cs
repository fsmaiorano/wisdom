using System.Net.Http.Headers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

public class CourseApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    public Task<CreateCourseResponse?> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default)
        => PostAsync<CreateCourseResponse>("/api/courses", request, ct);

    public async Task<GetLessonResponse?> CreateLessonAsync(
        Guid courseId, CreateLessonRequest request, string? videoFilePath = null, string? thumbnailUrl = null, CancellationToken ct = default)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(request.Title), "title");
        form.Add(new StringContent(request.Description), "description");
        form.Add(new StringContent(request.Position.ToString()), "position");
        form.Add(new StringContent(request.IsPreview.ToString().ToLower()), "isPreview");
        if (thumbnailUrl is not null)
            form.Add(new StringContent(thumbnailUrl), "thumbnailUrl");

        FileStream? fileStream = null;
        if (videoFilePath is not null && File.Exists(videoFilePath))
        {
            fileStream = File.OpenRead(videoFilePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/x-matroska");
            form.Add(fileContent, "video", Path.GetFileName(videoFilePath));
        }

        try
        {
            return await PostMultipartAsync<GetLessonResponse>($"/api/courses/{courseId}/lessons", form, ct);
        }
        finally
        {
            fileStream?.Dispose();
        }
    }

    public Task<GetCourseResponse?> PublishCourseAsync(Guid courseId, bool publish, CancellationToken ct = default)
        => PatchAsync<GetCourseResponse>($"/api/courses/{courseId}/publish", new PublishCourseRequest(publish), ct);

    public Task<List<GetCourseResponse>?> ListCoursesAsync(CancellationToken ct = default)
        => GetAsync<List<GetCourseResponse>>("/api/courses", ct);

    public Task<List<GetLessonResponse>?> GetLessonsAsync(Guid courseId, CancellationToken ct = default)
        => GetAsync<List<GetLessonResponse>>($"/api/courses/{courseId}/lessons", ct);
}
