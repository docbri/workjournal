namespace WorkJournalApi.Options;

public sealed class DiagnosticsOptions
{
    public const string SectionName = "Diagnostics";

    public string EnvironmentName { get; init; } = "Unknown";
    public bool EnableConfigEndpoint { get; init; }
}
