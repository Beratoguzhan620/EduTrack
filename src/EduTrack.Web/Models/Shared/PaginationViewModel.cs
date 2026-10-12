namespace EduTrack.Web.Models.Shared;

/// <summary>
/// Input for <c>Partials/_Pagination</c>. Page indexes are zero-based to match <c>Paged&lt;T&gt;</c>.
/// The partial keeps every other query-string value, so filters survive page changes.
/// </summary>
public sealed record PaginationViewModel(int Index, int Pages, int TotalCount, int PageSize)
{
    /// <summary>Query-string key that carries the page index (e.g. "Query.Index").</summary>
    public string PageParameter { get; init; } = "page";

    public bool HasPrevious => Index > 0;
    public bool HasNext => Index < Pages - 1;
    public int FirstItem => TotalCount == 0 ? 0 : Index * PageSize + 1;
    public int LastItem => Math.Min((Index + 1) * PageSize, TotalCount);

    /// <summary>Page numbers to render, with <c>null</c> marking a gap ("…").</summary>
    public IReadOnlyList<int?> VisiblePages(int window = 1)
    {
        var pages = new List<int?>();
        for (var i = 0; i < Pages; i++)
        {
            var isEdge = i == 0 || i == Pages - 1;
            var isNear = Math.Abs(i - Index) <= window;
            if (isEdge || isNear)
            {
                pages.Add(i);
            }
            else if (pages.Count > 0 && pages[^1] is not null)
            {
                pages.Add(null);
            }
        }

        return pages;
    }
}
