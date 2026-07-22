using System;
using System.Collections.Generic;
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
using SmartAttendance.Application.Features.Shifts.DTOs;

namespace SmartAttendance.Application.Features.Shifts.Queries;

public class GetShiftByIdQuery : IRequest<Result<ShiftDetailsDto>>
{
    public Guid ShiftId { get; set; }
}

public class GetShiftByIdQueryValidator : AbstractValidator<GetShiftByIdQuery>
{
    public GetShiftByIdQueryValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
    }
}

public class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, Result<ShiftDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetShiftByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ShiftDetailsDto>> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .AsNoTracking()
            .Where(s => s.Id == request.ShiftId)
            .ProjectTo<ShiftDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (shift == null)
        {
            return Result<ShiftDetailsDto>.Failure(new[] { "Shift not found." });
        }

        return Result<ShiftDetailsDto>.Success(shift);
    }
}
