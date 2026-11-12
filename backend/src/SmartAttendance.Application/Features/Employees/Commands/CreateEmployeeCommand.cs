using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.ValueObjects;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class CreateEmployeeCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public DateTime HireDate { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HireDate).LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Hire date cannot be in the future.");
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow.AddYears(-16)).WithMessage("Employee must be at least 16 years old.");
        RuleFor(x => x.NationalId).MaximumLength(50);
        RuleFor(x => x.PassportNumber).MaximumLength(50);

        RuleFor(x => x.Street).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.State).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
    }
}

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreateEmployeeCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // ── Validation ───────────────────────────────────────────────
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists) return Result<Guid>.Failure(new[] { "Company not found." });

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId && b.CompanyId == request.CompanyId, cancellationToken);
        if (!branchExists) return Result<Guid>.Failure(new[] { "Branch not found or does not belong to the company." });

        var departmentExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId && d.BranchId == request.BranchId, cancellationToken);
        if (!departmentExists) return Result<Guid>.Failure(new[] { "Department not found or does not belong to the branch." });

        var duplicateEmployeeNumber = await _context.Employees.AnyAsync(e => e.CompanyId == request.CompanyId && e.EmployeeNumber == request.EmployeeNumber, cancellationToken);
        if (duplicateEmployeeNumber) return Result<Guid>.Failure(new[] { "Employee number already exists in the company." });

        var emailVO = new Email(request.Email);

        var duplicateEmployeeEmail = await _context.Employees.AnyAsync(e => e.CompanyId == request.CompanyId && e.Email.Value == request.Email, cancellationToken);
        if (duplicateEmployeeEmail) return Result<Guid>.Failure(new[] { "Email already exists in this company." });

        // Prevent creating a user account with an already-taken email
        var userEmailExists = await _context.Users.AnyAsync(u => u.Email.Value == request.Email, cancellationToken);
        if (userEmailExists) return Result<Guid>.Failure(new[] { "A user account with this email already exists." });

        // ── Create Employee ──────────────────────────────────────────
        var phoneVO = new PhoneNumber(request.PhoneNumber);
        var addressVO = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);

        var employee = SmartAttendance.Domain.Entities.Employee.Create(
            request.CompanyId,
            request.EmployeeNumber,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.HireDate,
            request.JobTitle,
            request.BranchId,
            request.DepartmentId,
            request.AttendancePolicyId,
            emailVO,
            phoneVO,
            addressVO,
            request.MiddleName,
            request.PreferredName,
            request.NationalId,
            request.PassportNumber);

        _context.Employees.Add(employee);

        // ── Auto-create System User account ─────────────────────────
        // Username = local part of the email (before @), lowercased
        var username = request.Email.Split('@')[0].ToLowerInvariant();

        // Ensure username uniqueness by appending employee number if already taken
        var usernameExists = await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
        if (usernameExists)
            username = $"{username}.{request.EmployeeNumber.ToLowerInvariant()}";

        // Temporary password: FirstName@EmployeeNumber (e.g. John@EMP-001)
        var temporaryPassword = $"{request.FirstName}@{request.EmployeeNumber}";

        var domainUser = SmartAttendance.Domain.Entities.User.Create(
            request.CompanyId,
            username,
            emailVO);

        domainUser.Activate();

        _context.Users.Add(domainUser);
        await _context.SaveChangesAsync(cancellationToken);

        // Create the ASP.NET Identity account linked to the domain user
        var (identityResult, _) = await _identityService.CreateUserAsync(domainUser.Id, username, temporaryPassword, request.Email);

        if (!identityResult.Succeeded)
        {
            // Roll back both records on failure
            _context.Users.Remove(domainUser);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Failure(identityResult.Errors);
        }

        // Assign the default "Employee" role automatically
        await _identityService.UpdateUserRolesAsync(domainUser.Id, new[] { "Employee" });

        return Result<Guid>.Success(employee.Id);
    }
}
