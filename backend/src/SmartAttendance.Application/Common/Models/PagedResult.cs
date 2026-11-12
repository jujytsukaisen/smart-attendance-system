using System.Collections.Generic;

namespace SmartAttendance.Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = new List<T>();
    public PaginationResponse Pagination { get; set; } = new();
    public int UnreadCount { get; set; }
}
