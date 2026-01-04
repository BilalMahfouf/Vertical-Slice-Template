namespace VeterinaryApi.Common.Paginations;

public class PagedList<T>
{
    public IEnumerable<T> Item { get; private set; } = null!;
    public int TotalCount { get; private set; }
    public int PageSize { get; private set; }
    public int Page { get; private set; }
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;

    private PagedList()
    {
        
    }
    public static PagedList<T> Create(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        return new PagedList<T>
        {
            Item = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

}
