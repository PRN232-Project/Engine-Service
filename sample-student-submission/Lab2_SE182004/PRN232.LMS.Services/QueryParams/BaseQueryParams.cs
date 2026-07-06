namespace PRN232.LMS.Services.QueryParams;

public class BaseQueryParams
{
    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _size = 10;
    public int Size
    {
        get => _size;
        set => _size = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    public string? Sort { get; set; }
    public string? Search { get; set; }
    public string? Fields { get; set; }
    public string? Expand { get; set; }
}
