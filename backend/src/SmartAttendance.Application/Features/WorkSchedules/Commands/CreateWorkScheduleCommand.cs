using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class CreateWorkScheduleCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFlexible { get; set; }
    public double WeeklyWorkingHours { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
}

public class CreateWorkScheduleCommandValidator : AbstractValidator<CreateWorkScheduleCommand>
{
    public CreateWorkScheduleCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
        RuleFor(x => x.WeeklyWorkingHours).GreaterThan(0);
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowEarlyCheckInMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AllowLateCheckOutMinutes).GreaterThanOrEqualTo(0);
    }
}

public class CreateWorkScheduleCommandHandler : IRequestHandler<CreateWorkScheduleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkScheduleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
        {
            return Result<Guid>.Failure(new[] { "Company not found." });
        }

        var nameExists = await _context.WorkSchedules.AnyAsync(w => w.CompanyId == request.CompanyId && w.Name == request.Name, cancellationToken);
        if (nameExists)
        {
            return Result<Guid>.Failure(new[] { "A work schedule with this name already exists." });
        }

        var schedule = WorkSchedule.Create(
            request.CompanyId,
            request.Name,
            request.Description,
            request.IsFlexible,
            request.WeeklyWorkingHours,
            request.GracePeriodMinutes,
            request.AllowEarlyCheckInMinutes,
            request.AllowLateCheckOutMinutes);

        await _context.WorkSchedules.AddAsync(schedule, cancellationToken);

        return Result<Guid>.Success(schedule.Id);
    }
}

