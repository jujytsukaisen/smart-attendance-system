using System;
using FluentAssertions;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;
using SmartAttendance.Domain.ValueObjects;
using Xunit;

namespace SmartAttendance.UnitTests.Domain.Entities;

public class EmployeeTests
{
    private Employee CreateTestEmployee()
    {
        return Employee.Create(
            companyId: Guid.NewGuid(),
            employeeNumber: "EMP001",
            firstName: "John",
            lastName: "Doe",
            dateOfBirth: new DateTime(1990, 1, 1),
            gender: Gender.Male,
            hireDate: DateTime.UtcNow.AddYears(-1),
            jobTitle: "Software Engineer",
            branchId: Guid.NewGuid(),
            departmentId: Guid.NewGuid(),
            attendancePolicyId: Guid.NewGuid(),
            email: new Email("john.doe@example.com"),
            phoneNumber: new PhoneNumber("+1234567890"),
            address: new Address("123 St", "City", "State", "Country", "12345"));
    }

    [Fact]
    public void TransferToDepartment_ShouldThrowException_WhenAssigningDuplicateDepartment()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var currentDepartmentId = employee.DepartmentId;

        // Act
        Action act = () => employee.TransferToDepartment(currentDepartmentId);

        // Assert
        act.Should().Throw<InvalidEmployeeException>()
            .WithMessage("Cannot assign duplicate department.");
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenEmployeeIsActiveAdmin()
    {
        // Arrange
        var employee = CreateTestEmployee();
        // Employee is active by default after creation

        // Act
        Action act = () => employee.Deactivate(isAdmin: true);

        // Assert
        act.Should().Throw<InvalidEmployeeException>()
            .WithMessage("Cannot deactivate an active admin.");
    }
}
