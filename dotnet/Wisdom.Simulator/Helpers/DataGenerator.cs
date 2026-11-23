using Bogus;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Helpers;

public static class DataGenerator
{
    private static readonly Faker Faker = new("en");

    public static RegisterUserRequest FakeTeacher() => new(
        Email: Faker.Internet.Email(),
        Name: Faker.Name.FullName(),
        // Password: $"P@ss{Faker.Random.Number(1000, 9999)}!",
        Password: "123456!",
        Role: 1); // Teacher

    public static RegisterUserRequest FakeCustomer() => new(
        Email: Faker.Internet.Email(),
        Name: Faker.Name.FullName(),
        // Password: $"P@ss{Faker.Random.Number(1000, 9999)}!",
        Password: "123456!",
        Role: 2); // Customer

    public static CreateCourseRequest FakeCourse() => new(
        Title: $"{Faker.Hacker.Adjective()} {Faker.Hacker.Noun()} Masterclass",
        Description: Faker.Lorem.Sentence(10),
        Price: Math.Round((decimal)Faker.Random.Double(9.99, 199.99), 2));

    public static CreateLessonRequest FakeLesson(int position) => new(
        Title: $"Lesson {position}: {Faker.Hacker.Verb()} {Faker.Hacker.Noun()}",
        Description: Faker.Lorem.Sentence(8),
        Position: position,
        IsPreview: position == 1);

    /// <summary>Hand-written course used by the demo scenario (readable, unlike the Bogus one).</summary>
    public static CreateCourseRequest DemoCourse() => new(
        Title: $"Full-Stack Engineering Demo ({DateTime.Now:yyyy-MM-dd HH:mm})",
        Description: "A hands-on tour from architecture to production: services, data, messaging, delivery and observability.",
        Price: 49.90m);

    /// <summary>Lessons with different themes (kept to 6: each video takes ~2 min to transcode). The first one is a free preview.</summary>
    public static CreateLessonRequest[] DemoLessons() =>
    [
        new("Welcome and course roadmap",
            "What you will build, how the modules connect and how to get the most out of the course.", 1, true),
        new("Designing microservices boundaries",
            "Bounded contexts, ownership of data and when a monolith is still the right answer.", 2, false),
        new("Persistence with PostgreSQL and EF Core",
            "Modelling aggregates, migrations, indexes and avoiding the classic N+1 trap.", 3, false),
        new("Asynchronous messaging with RabbitMQ",
            "Exchanges, queues, dead-letter handling, idempotent consumers and the outbox pattern.", 4, false),
        new("Securing APIs with Keycloak and JWT",
            "Single sign-on, token validation at the gateway and forwarding identity to backends.", 5, false),
        new("Video pipelines: upload, transcode, stream",
            "How a lesson video becomes 480p, 720p and 1080p renditions served with HTTP range requests.", 6, false),
    ];

    public static string FakeThumbnailUrl()
        => Faker.Image.PicsumUrl(800, 600);

    public static int FakeWatchedSeconds(int maxSeconds = 600)
        => Faker.Random.Int(30, maxSeconds);

    /// <summary>
    /// Walks up from the binary directory until a sibling "mocks/" folder is found,
    /// then returns all .mkv files inside it sorted by name.
    /// </summary>
    public static string[] FindMockVideos()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var mocks = Path.Combine(dir.FullName, "mocks");
            if (Directory.Exists(mocks))
                return [.. Directory.EnumerateFiles(mocks, "*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase)
                        || file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                    .Order()
                ];
            dir = dir.Parent;
        }
        return [];
    }
}
