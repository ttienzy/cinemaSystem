namespace Cinema.Shared.Models;

public class PaginatedResponse<T>
{
    private const int MaxPageSize = 100;

    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResponse<T> Create(
        List<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var safePage = Math.Max(1, pageNumber);
        var safeSize = Math.Clamp(pageSize, 1, MaxPageSize);

        return new PaginatedResponse<T>
        {
            Items = items.AsReadOnly(),
            TotalCount = totalCount,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }
}