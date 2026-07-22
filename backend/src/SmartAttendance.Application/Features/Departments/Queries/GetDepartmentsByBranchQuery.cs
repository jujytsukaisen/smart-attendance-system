using System;
using System.Collections.Generic;
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
using SmartAttendance.Application.Features.Departments.DTOs;

namespace SmartAttendance.Application.Features.Departments.Queries;

public class GetDepartmentsByBranchQuery : IRequest<Result<List<DepartmentDto>>>
{
    public Guid BranchId { get; set; }
}

public class GetDepartmentsByBranchQueryValidator : AbstractValidator<GetDepartmentsByBranchQuery>
{
    public GetDepartmentsByBranchQueryValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class GetDepartmentsByBranchQueryHandler : IRequestHandler<GetDepartmentsByBranchQuery, Result<List<DepartmentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDepartmentsByBranchQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<DepartmentDto>>> Handle(GetDepartmentsByBranchQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Departments
            .AsNoTracking()
            .Where(d => d.BranchId == request.BranchId)
            .OrderBy(d => d.Name)
            .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<DepartmentDto>>.Success(items);
    }
}
