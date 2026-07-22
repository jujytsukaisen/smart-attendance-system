using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Features.Employees.Queries;
using SmartAttendance.Application.Features.Employees.DTOs;
using SmartAttendance.Domain.Entities;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Employees.Queries;

public class GetEmployeeByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly IMapper _mapper;
    private readonly GetEmployeeByIdQueryHandler _handler;

    public GetEmployeeByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        
        var mapperConfig = new MapperConfiguration(c => 
        {
            c.AddProfile<MappingProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetEmployeeByIdQueryHandler(_contextMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmployeeNotFound()
    {
        var command = new GetEmployeeByIdQuery { EmployeeId = Guid.NewGuid() };
        var employees = new List<Employee>();
        var mockDbSet = employees.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Employees).Returns(mockDbSet.Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithDto_WhenEmployeeExists()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new GetEmployeeByIdQuery { EmployeeId = employeeId };
        
        // Use FormatterServices to bypass constructor rules just for this Mock setup
        var employee = (Employee)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Employee));
        typeof(Employee).GetProperty("Id")!.SetValue(employee, employeeId);
        typeof(Employee).GetProperty("FirstName")!.SetValue(employee, "John");
        typeof(Employee).GetProperty("LastName")!.SetValue(employee, "Doe");
        var emailObj = new SmartAttendance.Domain.ValueObjects.Email("john@example.com");
        typeof(Employee).GetProperty("Email")!.SetValue(employee, emailObj);
        
        var phoneObj = new SmartAttendance.Domain.ValueObjects.PhoneNumber("1234567890");
        typeof(Employee).GetProperty("PhoneNumber")!.SetValue(employee, phoneObj);

        var addressObj = new SmartAttendance.Domain.ValueObjects.Address("Street", "City", "State", "Country", "Zip");
        typeof(Employee).GetProperty("Address")!.SetValue(employee, addressObj);


        var employees = new List<Employee> { employee };
        var mockDbSet = employees.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Employees).Returns(mockDbSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FirstName.Should().Be("John");
    }
}
