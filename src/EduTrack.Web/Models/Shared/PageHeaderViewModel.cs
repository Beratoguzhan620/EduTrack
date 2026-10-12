using Microsoft.AspNetCore.Html;

namespace EduTrack.Web.Models.Shared;

/// <summary>Title block rendered at the top of a page by <c>Partials/_PageHeader</c>.</summary>
public sealed record PageHeaderViewModel(string Title, string? Subtitle = null)
{
    /// <summary>Optional breadcrumb trail; the last item is rendered as the current page.</summary>
    public IReadOnlyList<BreadcrumbItem> Breadcrumbs { get; init; } = [];

    /// <summary>
    /// Optional buttons shown on the right, written as a Razor template:
    /// <c>Actions = @&lt;text&gt;&lt;a class="btn btn-primary"&gt;…&lt;/a&gt;&lt;/text&gt;</c>.
    /// </summary>
    public Func<object?, IHtmlContent>? Actions { get; init; }
}

public sealed record BreadcrumbItem(string Text, string? Url = null);
