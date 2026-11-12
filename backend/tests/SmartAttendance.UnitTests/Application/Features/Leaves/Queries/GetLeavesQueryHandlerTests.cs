using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Features.Leaves.Queries;
using SmartAttendance.Domain.Entities;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Leaves.Queries;

public class GetLeavesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetLeavesQueryHandler _handler;

    public GetLeavesQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetLeavesQueryHandler(_contextMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoLeavesExist()
    {
        // Arrange
        var leaves = new List<Leave>();
        var mockDbSet = leaves.AsQueryable().BuildMockDbSet();
        
        _contextMock.Setup(c => c.Leaves).Returns(mockDbSet.Object);

        var query = new GetLeavesQuery { PageNumber = 1, PageSize = 10 };

        _mapperMock.Setup(m => m.Map<List<LeaveListDto>>(It.IsAny<List<Leave>>()))
            .Returns(new List<LeaveListDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Pagination.TotalCount.Should().Be(0);
        result.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResults_WhenLeavesExist()
    {
        // Arrange
        // Bypass constructor rules for mock data
        var leave1 = (Leave)RuntimeHelpers.GetUninitializedObject(typeof(Leave));
        var leave2 = (Leave)RuntimeHelpers.GetUninitializedObject(typeof(Leave));
        var leave3 = (Leave)RuntimeHelpers.GetUninitializedObject(typeof(Leave));

        var leaves = new List<Leave> { leave1, leave2, leave3 };
        var mockDbSet = leaves.AsQueryable().BuildMockDbSet();
        
        _contextMock.Setup(c => c.Leaves).Returns(mockDbSet.Object);

        var query = new GetLeavesQuery { PageNumber = 2, PageSize = 2 };

        // For page 2, size 2, it skips 2, takes 2, so it should map just 1 item (leave3).
        var mappedDto = new LeaveListDto { Id = Guid.NewGuid() };
        _mapperMock.Setup(m => m.Map<List<LeaveListDto>>(It.IsAny<List<Leave>>()))
            .Returns(new List<LeaveListDto> { mappedDto });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Pagination.TotalCount.Should().Be(3);
        result.Data.Items.Count.Should().Be(1);
        result.Data.Pagination.PageNumber.Should().Be(2);
        result.Data.Pagination.HasPreviousPage.Should().BeTrue();
    }
}
