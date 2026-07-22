using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Companies.DTOs;

namespace SmartAttendance.Application.Features.Companies.Queries;

public class GetCompaniesQuery : PaginationRequest, IRequest<Result<PagedResult<CompanyListDto>>>
{
    public string? Name { get; set; }
    public string? RegistrationNumber { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class GetCompaniesQueryValidator : AbstractValidator<GetCompaniesQuery>
{
    public GetCompaniesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, Result<PagedResult<CompanyListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCompaniesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CompanyListDto>>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Companies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            query = query.Where(c => c.Name.Contains(name) || c.LegalName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(request.RegistrationNumber))
        {
            var regNum = request.RegistrationNumber.Trim();
            query = query.Where(c => c.RegistrationNumber.Contains(regNum));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "registrationnumber" => request.SortDescending ? query.OrderByDescending(c => c.RegistrationNumber) : query.OrderBy(c => c.RegistrationNumber),
            "foundeddate" => request.SortDescending ? query.OrderByDescending(c => c.FoundedDate) : query.OrderBy(c => c.FoundedDate),
            _ => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name) // default
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<CompanyListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<CompanyListDto>
        {
            Items = items,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<CompanyListDto>>.Success(result);
    }
}
