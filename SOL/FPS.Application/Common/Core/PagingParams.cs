using Template.Domain.Enums;

namespace Template.Application.Common.Core;

public class PagingParams
{
    public int PageSize { get; set; } = 10;
    public int PageIndex { get; set; } = 1;
    public string? Column { get; set; }
    public SortType SortType { get; set; }
}

