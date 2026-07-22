using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class ChangeDescriptionCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
    public string NewDescription { get; set; } = string.Empty;
}

public class ChangeDescriptionCommandValidator : AbstractValidator<ChangeDescriptionCommand>
{
    public ChangeDescriptionCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.NewDescription).MaximumLength(250);
    }
}

public class ChangeDescriptionCommandHandler : IRequestHandler<ChangeDescriptionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeDescriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeDescriptionCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        if (department == null) return Result.Failure(new[] { "Department not found." });
        
        department.ChangeDescription(request.NewDescription);
        
        _context.Departments.Update(department);

        return Result.Success();
    }
}

