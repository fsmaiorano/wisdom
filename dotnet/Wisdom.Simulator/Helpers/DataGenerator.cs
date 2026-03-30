using Bogus;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Helpers;

public static class DataGenerator
{
    private static readonly Faker Faker = new("en");

    public static RegisterUserRequest FakeTeacher() => new(
        Email: Faker.Internet.Email(),
        Name: Faker.Name.FullName(),
        Password: $"P@ss{Faker.Random.Number(1000, 9999)}!",
        Role: 1); // Teacher

    public static RegisterUserRequest FakeCustomer() => new(
        Email: Faker.Internet.Email(),
        Name: Faker.Name.FullName(),
        Password: $"P@ss{Faker.Random.Number(1000, 9999)}!",
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
                return Directory.GetFiles(mocks, "*.mkv", SearchOption.TopDirectoryOnly)
                    .Order()
                    .ToArray();
            dir = dir.Parent;
        }
        return [];
    }
}
