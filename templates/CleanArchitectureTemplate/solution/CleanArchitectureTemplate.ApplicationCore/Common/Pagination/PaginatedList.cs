namespace CleanArchitectureTemplate.ApplicationCore.Common.Pagination;

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int FilteredCount { get; set; }

    public PaginatedList(
        List<T> items,
        int count,
        int pageIndex,
        int pageSize,
        int totalRecords,
        int filteredCount
    )
    {
        FilteredCount = filteredCount;
        TotalRecords = totalRecords;
        PageIndex = pageIndex;
        PageSize = pageSize;

        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;
}
