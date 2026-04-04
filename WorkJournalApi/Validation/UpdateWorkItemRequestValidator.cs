using WorkJournalApi.Contracts;

namespace WorkJournalApi.Validation;

public sealed class UpdateWorkItemRequestValidator
{
    public ValidationResult Validate(UpdateWorkItemRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            result.AddError(nameof(request.Title), "Title is required.");
        }
        else if (request.Title.Trim().Length > 120)
        {
            result.AddError(nameof(request.Title), "Title cannot exceed 120 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Trim().Length > 1000)
        {
            result.AddError(nameof(request.Notes), "Notes cannot exceed 1000 characters.");
        }

        return result;
    }
}
