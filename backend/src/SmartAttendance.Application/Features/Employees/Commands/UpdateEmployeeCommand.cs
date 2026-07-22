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

public class UpdateEmployeeCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyRelationship { get; set; }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow.AddYears(-16)).WithMessage("Employee must be at least 16 years old.");
        RuleFor(x => x.NationalId).MaximumLength(50);
        RuleFor(x => x.PassportNumber).MaximumLength(50);
        
        RuleFor(x => x.Street).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.State).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
        
        RuleFor(x => x.EmergencyContactName).MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(20);
        RuleFor(x => x.EmergencyRelationship).MaximumLength(100);
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        var duplicateEmail = await _context.Employees.AnyAsync(e => e.Id != request.EmployeeId && e.Email.Value == request.Email, cancellationToken);
        if (duplicateEmail)
            return Result.Failure(new[] { "Email already exists." });

        employee.UpdatePersonalInformation(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.MiddleName,
            request.PreferredName,
            request.NationalId,
            request.PassportNumber);

        var emailVO = new Email(request.Email);
        var phoneVO = new PhoneNumber(request.PhoneNumber);
        var addressVO = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);

        employee.UpdateContactInformation(emailVO, phoneVO, addressVO);

        employee.ChangeJobTitle(request.JobTitle);

        if (!string.IsNullOrWhiteSpace(request.EmergencyContactPhone))
        {
            var emergencyPhoneVO = new PhoneNumber(request.EmergencyContactPhone);
            employee.UpdateEmergencyContact(request.EmergencyContactName, emergencyPhoneVO, request.EmergencyRelationship);
        }
        else
        {
            employee.UpdateEmergencyContact(request.EmergencyContactName, null, request.EmergencyRelationship);
        }

        return Result.Success();
    }
}



