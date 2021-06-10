/* Represents the configuration of the simulation */
public class NavConfig
{
    public string ParticipantName { get; }
    public Path Path { get; }
    public SimulationManager.AdviceName Advice { get; }

    public NavConfig(string participantName, Path path, SimulationManager.AdviceName advice)
    {
        ParticipantName = participantName;
        Path = path;
        Advice = advice;
    }
}