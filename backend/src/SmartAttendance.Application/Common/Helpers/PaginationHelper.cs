using System;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Helpers;

public static class PaginationHelper
{
    public static PaginationResponse CreatePaginationResponse(int totalCount, int pageNumber, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginationResponse
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}
