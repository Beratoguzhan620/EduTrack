namespace EduTrack.Web.Models.Shared;

/// <summary>Placeholder shown by <c>Partials/_EmptyState</c> when a list has nothing to display.</summary>
public sealed record EmptyStateViewModel(string Title, string Description, string Icon = "bi-inbox")
{
    public string? ActionText { get; init; }
    public string? ActionUrl { get; init; }

    /// <summary>When set, the action button opens this Bootstrap modal instead of navigating.</summary>
    public string? ActionModalTarget { get; init; }
}
