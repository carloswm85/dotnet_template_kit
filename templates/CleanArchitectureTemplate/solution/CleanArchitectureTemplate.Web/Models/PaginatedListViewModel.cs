using CleanArchitectureTemplate.ApplicationCore.Common.Pagination;

namespace CleanArchitectureTemplate.Web.Models;

public class PaginatedListViewModel<T>
    where T : class
{
    public PaginatedList<T> PaginatedList;
    public string CurrentFilter;
    public string CurrentSort;
    public string SortColumnOne;
    public string SortColumnTwo;
    public int PageSize;

    public PaginatedListViewModel(
        PaginatedList<T> paginatedList,
        string currentFilter,
        string currentSort,
        string sortColumnOne,
        string sortColumnTwo,
        int pageSize
    )
    {
        PaginatedList = paginatedList;
        CurrentFilter = currentFilter;
        CurrentSort = currentSort;
        SortColumnOne = sortColumnOne;
        SortColumnTwo = sortColumnTwo;
        PageSize = pageSize;
    }
}
