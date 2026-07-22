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
using SmartAttendance.Application.Features.Departments.DTOs;

namespace SmartAttendance.Application.Features.Departments.Queries;

public class GetDepartmentByIdQuery : IRequest<Result<DepartmentDetailsDto>>
{
    public Guid DepartmentId { get; set; }
}

public class GetDepartmentByIdQueryValidator : AbstractValidator<GetDepartmentByIdQuery>
{
    public GetDepartmentByIdQueryValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<DepartmentDetailsDto>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == request.DepartmentId)
            .ProjectTo<DepartmentDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return Result<DepartmentDetailsDto>.Failure(new[] { "Department not found." });

        return Result<DepartmentDetailsDto>.Success(dto);
    }
}
