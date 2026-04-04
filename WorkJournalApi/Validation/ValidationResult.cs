namespace WorkJournalApi.Validation;

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public Dictionary<string, string[]> Errors { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void AddError(string field, string message)
    {
        if (Errors.TryGetValue(field, out var existing))
        {
            Errors[field] = [.. existing, message];
            return;
        }

        Errors[field] = [message];
    }
}
