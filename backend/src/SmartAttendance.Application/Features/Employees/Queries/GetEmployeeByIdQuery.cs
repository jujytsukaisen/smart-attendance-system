using System;
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

public class GetEmployeeByIdQuery : IRequest<Result<EmployeeDetailsDto>>
{
    public Guid EmployeeId { get; set; }
}

public class GetEmployeeByIdQueryValidator : AbstractValidator<GetEmployeeByIdQuery>
{
    public GetEmployeeByIdQueryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEmployeeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDetailsDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Where(x => x.Id == request.EmployeeId)
            .ProjectTo<EmployeeDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (employee == null)
        {
            return Result<EmployeeDetailsDto>.Failure(new[] { $"Employee with ID {request.EmployeeId} was not found." });
        }

        return Result<EmployeeDetailsDto>.Success(employee);
    }
}
