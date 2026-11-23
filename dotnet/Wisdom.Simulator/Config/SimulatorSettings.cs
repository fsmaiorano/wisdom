namespace Wisdom.Simulator.Config;

public class SimulatorSettings
{
    /// <summary>
    /// Single entry point for all API calls. All routes go through the BFF,
    /// which validates the JWT and forwards internal auth headers to backends.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000"; //Local
    // public string BaseUrl { get; set; } = "http://localhost:6100"; //Docker
    //BASE_URL=http://localhost:6100 dotnet run

    public int LessonsPerCourse  { get; set; } = 5;
    public int NumberOfCustomers { get; set; } = 3;
    public int WatchDelayMs      { get; set; } = 1000;

    // Accounts created by the SeedDemoUsers migration / Keycloak realm export.
    public string SeedTeacherEmail    { get; set; } = "teacher@wisdom.dev";
    public string SeedTeacherPassword { get; set; } = "demoteacher";
    public string SeedStudentEmail    { get; set; } = "customer@wisdom.dev";
    public string SeedStudentPassword { get; set; } = "democustomer";

    /// <summary>How long Scenario 6 waits for the Media Worker to finish transcoding.</summary>
    public int MediaProcessingTimeoutSeconds { get; set; } = 1200;
}
