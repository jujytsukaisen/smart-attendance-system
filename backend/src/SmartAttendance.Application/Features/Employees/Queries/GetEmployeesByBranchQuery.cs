using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Employees.DTOs;

namespace SmartAttendance.Application.Features.Employees.Queries;

public class GetEmployeesByBranchQuery : PaginationRequest, IRequest<Result<PagedResult<EmployeeListDto>>>
{
    public Guid BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetEmployeesByBranchQueryValidator : AbstractValidator<GetEmployeesByBranchQuery>
{
    public GetEmployeesByBranchQueryValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetEmployeesByBranchQueryHandler : IRequestHandler<GetEmployeesByBranchQuery, Result<PagedResult<EmployeeListDto>>>
{
    private readonly IMediator _mediator;

    public GetEmployeesByBranchQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<PagedResult<EmployeeListDto>>> Handle(GetEmployeesByBranchQuery request, CancellationToken cancellationToken)
    {
        // Delegate to the main GetEmployeesQuery
        var query = new GetEmployeesQuery
        {
            BranchId = request.BranchId,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy,
            SortDesc = request.SortDesc,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return await _mediator.Send(query, cancellationToken);
    }
}
