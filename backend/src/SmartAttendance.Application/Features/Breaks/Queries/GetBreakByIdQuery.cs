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
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Breaks.Queries;

using SmartAttendance.Application.Features.Breaks.DTOs;

public class GetBreakByIdQuery : IRequest<Result<BreakDetailsDto>>
{
    public Guid BreakId { get; set; }
}

public class GetBreakByIdQueryValidator : AbstractValidator<GetBreakByIdQuery>
{
    public GetBreakByIdQueryValidator()
    {
        RuleFor(x => x.BreakId).NotEmpty();
    }
}

public class GetBreakByIdQueryHandler : IRequestHandler<GetBreakByIdQuery, Result<BreakDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBreakByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<BreakDetailsDto>> Handle(GetBreakByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.Breaks
            .AsNoTracking()
            .Where(b => b.Id == request.BreakId)
            .ProjectTo<BreakDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return Result<BreakDetailsDto>.Failure(new[] { "Break not found." });

        return Result<BreakDetailsDto>.Success(dto);
    }
}
