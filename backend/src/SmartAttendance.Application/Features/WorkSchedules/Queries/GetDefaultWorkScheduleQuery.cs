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
using SmartAttendance.Application.Features.WorkSchedules.DTOs;

namespace SmartAttendance.Application.Features.WorkSchedules.Queries;

public class GetDefaultWorkScheduleQuery : IRequest<Result<WorkScheduleDetailsDto>>
{
}

public class GetDefaultWorkScheduleQueryValidator : AbstractValidator<GetDefaultWorkScheduleQuery>
{
    public GetDefaultWorkScheduleQueryValidator()
    {
    }
}

public class GetDefaultWorkScheduleQueryHandler : IRequestHandler<GetDefaultWorkScheduleQuery, Result<WorkScheduleDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDefaultWorkScheduleQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<WorkScheduleDetailsDto>> Handle(GetDefaultWorkScheduleQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.WorkSchedules
            .AsNoTracking()
            .Where(w => w.IsDefault)
            .ProjectTo<WorkScheduleDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return Result<WorkScheduleDetailsDto>.Failure(new[] { "Default work schedule not found." });

        return Result<WorkScheduleDetailsDto>.Success(dto);
    }
}
