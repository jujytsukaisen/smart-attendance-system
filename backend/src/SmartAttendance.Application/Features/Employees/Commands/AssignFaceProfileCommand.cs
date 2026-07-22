using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class AssignFaceProfileCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public string FaceProfileReference { get; set; } = string.Empty;
}

public class AssignFaceProfileCommandValidator : AbstractValidator<AssignFaceProfileCommand>
{
    public AssignFaceProfileCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.FaceProfileReference).NotEmpty();
    }
}

public class AssignFaceProfileCommandHandler : IRequestHandler<AssignFaceProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignFaceProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignFaceProfileCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        if (!Guid.TryParse(request.FaceProfileReference, out var faceProfileId))
            return Result.Failure(new[] { "Invalid FaceProfileReference. Must be a Guid." });

        employee.AssignFaceProfile(faceProfileId);

        return Result.Success();
    }
}

