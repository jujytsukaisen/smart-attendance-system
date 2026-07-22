using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Breaks.Commands;

public class CreateBreakCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public Guid ShiftId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BreakType BreakType { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsPaid { get; set; }
    public bool IsMandatory { get; set; }
}

public class CreateBreakCommandValidator : AbstractValidator<CreateBreakCommand>
{
    public CreateBreakCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
    }
}

public class CreateBreakCommandHandler : IRequestHandler<CreateBreakCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateBreakCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateBreakCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.Shifts
            .Include(s => s.Breaks)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId && s.CompanyId == request.CompanyId, cancellationToken);

        if (shift == null)
            return Result<Guid>.Failure(new[] { "Shift not found or does not belong to the company." });

        var newBreak = Break.Create(
            request.CompanyId,
            request.ShiftId,
            request.Name,
            request.Description,
            request.StartTime,
            request.EndTime,
            request.DurationMinutes,
            request.BreakType,
            request.IsPaid,
            request.IsMandatory);

        shift.AddBreak(newBreak);

        await _context.Breaks.AddAsync(newBreak, cancellationToken);

        return Result<Guid>.Success(newBreak.Id);
    }
}

