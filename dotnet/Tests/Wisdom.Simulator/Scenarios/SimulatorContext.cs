using Wisdom.Simulator.Clients;
using Wisdom.Simulator.Config;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Holds the API clients and shared state produced across scenarios.
/// All clients target the BFF — it validates the JWT and forwards
/// internal auth headers to the backends.
/// </summary>
public class SimulatorContext
{
    public SimulatorSettings Settings { get; }
    public UserApiClient UserClient { get; }
    public CourseApiClient CourseClient { get; }
    public OrderApiClient OrderClient { get; }
    public LearningApiClient LearningClient { get; }

    // Populated during scenario execution
    public RegisterUserRequest? TeacherCredentials { get; set; }
    public LoginUserResponse? TeacherToken { get; set; }
    public CreateCourseResponse? Course { get; set; }
    public List<GetLessonResponse> Lessons { get; } = [];
    public List<(RegisterUserRequest Credentials, LoginUserResponse Token)> Customers { get; } = [];

    public SimulatorContext(SimulatorSettings settings)
    {
        Settings = settings;
        UserClient     = new UserApiClient(settings.BaseUrl);
        CourseClient   = new CourseApiClient(settings.BaseUrl);
        OrderClient    = new OrderApiClient(settings.BaseUrl);
        LearningClient = new LearningApiClient(settings.BaseUrl);
    }
}
