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

public class GetEmployeesByDepartmentQuery : PaginationRequest, IRequest<Result<PagedResult<EmployeeListDto>>>
{
    public Guid DepartmentId { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetEmployeesByDepartmentQueryValidator : AbstractValidator<GetEmployeesByDepartmentQuery>
{
    public GetEmployeesByDepartmentQueryValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetEmployeesByDepartmentQueryHandler : IRequestHandler<GetEmployeesByDepartmentQuery, Result<PagedResult<EmployeeListDto>>>
{
    private readonly IMediator _mediator;

    public GetEmployeesByDepartmentQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<PagedResult<EmployeeListDto>>> Handle(GetEmployeesByDepartmentQuery request, CancellationToken cancellationToken)
    {
        // Delegate to the main GetEmployeesQuery
        var query = new GetEmployeesQuery
        {
            DepartmentId = request.DepartmentId,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy,
            SortDesc = request.SortDesc,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return await _mediator.Send(query, cancellationToken);
    }
}
