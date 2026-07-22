using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Departments.DTOs;

namespace SmartAttendance.Application.Features.Departments.Queries;

public class GetDepartmentsByCompanyQuery : IRequest<Result<List<DepartmentDto>>>
{
    public Guid CompanyId { get; set; }
}

public class GetDepartmentsByCompanyQueryValidator : AbstractValidator<GetDepartmentsByCompanyQuery>
{
    public GetDepartmentsByCompanyQueryValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class GetDepartmentsByCompanyQueryHandler : IRequestHandler<GetDepartmentsByCompanyQuery, Result<List<DepartmentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDepartmentsByCompanyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<DepartmentDto>>> Handle(GetDepartmentsByCompanyQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .Where(x => x.CompanyId == request.CompanyId)
            .OrderBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<List<DepartmentDto>>(departments);
        return Result<List<DepartmentDto>>.Success(dtos);
    }
}
