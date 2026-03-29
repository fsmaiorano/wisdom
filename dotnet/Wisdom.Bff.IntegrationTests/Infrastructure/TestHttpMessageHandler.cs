using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Wisdom.Bff.IntegrationTests.Infrastructure;

/// <summary>
/// In-memory HTTP message handler that serves canned responses for specific paths.
/// Used to mock backend APIs (Course, Order, Learning) during integration tests.
/// </summary>
public sealed class TestHttpMessageHandler : HttpMessageHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly List<(Func<HttpRequestMessage, bool> Match, Func<HttpResponseMessage> Build)> _handlers = [];

    public void SetupGet<T>(string path, T body)
        => Register(r => r.Method == HttpMethod.Get && r.RequestUri!.AbsolutePath == path,
                    () => OkJson(body));

    public void SetupPost<T>(string path, T body)
        => Register(r => r.Method == HttpMethod.Post && r.RequestUri!.AbsolutePath == path,
                    () => OkJson(body));

    public void SetupNotFound(string path)
        => Register(r => r.RequestUri!.AbsolutePath == path,
                    () => new HttpResponseMessage(HttpStatusCode.NotFound));

    public void Reset() => _handlers.Clear();

    private void Register(Func<HttpRequestMessage, bool> match, Func<HttpResponseMessage> build)
        => _handlers.Insert(0, (match, build));   // last-registered wins

    private static HttpResponseMessage OkJson<T>(T body) => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(body, options: JsonOptions)
    };

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var (match, build) in _handlers)
            if (match(request))
                return Task.FromResult(build());

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
