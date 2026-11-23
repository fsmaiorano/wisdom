using System.Net;
using System.Net.Http.Headers;
using Wisdom.Streaming.IntegrationTests.Fixtures;
using Wisdom.Tests.Common.Auth;
using Wisdom.Tests.Common.Builders;
namespace Wisdom.Streaming.IntegrationTests.Playback;

public class PlaybackTests : IClassFixture<StreamingApiFixture>
{
    private static readonly byte[] Video = Enumerable.Range(0, 1000).Select(i => (byte)(i % 251)).ToArray();

    private readonly StreamingApiFixture _fixture;
    private readonly HttpClient _client;

    public PlaybackTests(StreamingApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AuthHelper.TokenForEmail(BogusFactory.Email()));
    }

    private Guid SeedReadyMedia(string quality = "720p")
    {
        var id = Guid.NewGuid();
        _fixture.StatusReader.Statuses[id] = "Ready";
        _fixture.Storage.Objects[$"media/{id}/{quality}.mp4"] = Video;
        return id;
    }

    [Fact]
    public async Task Health_Returns200_WithoutAuthentication()
    {
        var response = await _fixture.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WithoutAuthentication_Returns401()
    {
        var id = SeedReadyMedia();

        var response = await _fixture.CreateClient().GetAsync($"/stream/{id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WithInvalidQuality_Returns400()
    {
        var id = SeedReadyMedia();

        var response = await _client.GetAsync($"/stream/{id}?quality=4k");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WithUnknownMedia_Returns404()
    {
        var response = await _client.GetAsync($"/stream/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WithMediaNotReady_Returns404()
    {
        var id = Guid.NewGuid();
        _fixture.StatusReader.Statuses[id] = "Processing";

        var response = await _client.GetAsync($"/stream/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WhenFileIsMissing_Returns404()
    {
        var id = Guid.NewGuid();
        _fixture.StatusReader.Statuses[id] = "Ready";

        var response = await _client.GetAsync($"/stream/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WithoutRange_ReturnsFullVideoWithDefaultQuality()
    {
        var id = SeedReadyMedia("720p");

        var response = await _client.GetAsync($"/stream/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("video/mp4", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("bytes", response.Headers.AcceptRanges.Single());
        Assert.Equal(Video.Length, response.Content.Headers.ContentLength);
        Assert.Equal(Video, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Stream_WithExplicitQuality_ReadsThatRendition()
    {
        var id = SeedReadyMedia("1080p");

        var response = await _client.GetAsync($"/stream/{id}?quality=1080p");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Stream_WithRange_Returns206AndRequestedBytes()
    {
        var id = SeedReadyMedia();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/stream/{id}");
        request.Headers.Range = new RangeHeaderValue(100, 199);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(new ContentRangeHeaderValue(100, 199, Video.Length), response.Content.Headers.ContentRange);
        Assert.Equal(100, response.Content.Headers.ContentLength);
        Assert.Equal(Video[100..200], await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Stream_WithSuffixRange_ReturnsLastBytes()
    {
        var id = SeedReadyMedia();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/stream/{id}");
        request.Headers.Range = new RangeHeaderValue(null, 50);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(Video[^50..], await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Stream_WithRangeBeyondFile_Returns416()
    {
        var id = SeedReadyMedia();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/stream/{id}");
        request.Headers.Range = new RangeHeaderValue(5000, 6000);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
        Assert.Equal($"bytes */{Video.Length}", response.Content.Headers.ContentRange?.ToString());
    }

    [Fact]
    public async Task Stream_WithHead_ReturnsHeadersOnly()
    {
        var id = SeedReadyMedia();

        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"/stream/{id}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Video.Length, response.Content.Headers.ContentLength);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }
}
