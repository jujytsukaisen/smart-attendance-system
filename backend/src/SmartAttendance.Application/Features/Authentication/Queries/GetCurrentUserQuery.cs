using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Authentication.Queries;

public class GetCurrentUserQuery : IRequest<Result<UserDto>>
{
}

public class UserDto : IMapFrom<User>
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.ToString()));
    }
}

public class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserQueryValidator()
    {
    }
}

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext context, IMapper mapper)
    {
        _currentUserService = currentUserService;
        _context = context;
        _mapper = mapper;
    }

        public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Result<UserDto>.Failure(new[] { "User is not authenticated." });
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userGuid, cancellationToken);
        if (user == null) return Result<UserDto>.Failure(new[] { "User not found." });

        var dto = new UserDto
        {
            Id = userId,
            Username = user.Username,
            Email = user.Email.Value
        };

        return Result<UserDto>.Success(dto);
    }
}


