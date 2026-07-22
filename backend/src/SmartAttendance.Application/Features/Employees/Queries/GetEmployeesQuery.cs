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

public class GetEmployeesQuery : PaginationRequest, IRequest<Result<PagedResult<EmployeeListDto>>>
{
    public string? SearchTerm { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetEmployeesQueryValidator : AbstractValidator<GetEmployeesQuery>
{
    public GetEmployeesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<PagedResult<EmployeeListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEmployeesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<EmployeeListDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Employees.AsNoTracking();

        if (request.CompanyId.HasValue && request.CompanyId.Value != Guid.Empty)
        {
            query = query.Where(x => x.CompanyId == request.CompanyId.Value);
        }

        if (request.BranchId.HasValue && request.BranchId.Value != Guid.Empty)
        {
            query = query.Where(x => x.BranchId == request.BranchId.Value);
        }

        if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var cleanSearchTerm = request.SearchTerm.Trim();
            query = query.Where(x => 
                x.FirstName.Contains(cleanSearchTerm) ||
                x.LastName.Contains(cleanSearchTerm) ||
                x.EmployeeNumber.Contains(cleanSearchTerm) ||
                x.Email.Value.Contains(cleanSearchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "employeenumber" => request.SortDesc ? query.OrderByDescending(x => x.EmployeeNumber) : query.OrderBy(x => x.EmployeeNumber),
            "firstname" => request.SortDesc ? query.OrderByDescending(x => x.FirstName) : query.OrderBy(x => x.FirstName),
            "lastname" => request.SortDesc ? query.OrderByDescending(x => x.LastName) : query.OrderBy(x => x.LastName),
            "email" => request.SortDesc ? query.OrderByDescending(x => x.Email.Value) : query.OrderBy(x => x.Email.Value),
            _ => request.SortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var dtos = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<EmployeeListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<EmployeeListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            }
        };
        
        return Result<PagedResult<EmployeeListDto>>.Success(result);
    }
}
