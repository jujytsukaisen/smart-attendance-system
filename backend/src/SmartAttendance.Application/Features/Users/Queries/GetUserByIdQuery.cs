using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<Result<UserDetailsDto>>
{
    public Guid UserId { get; set; }
}

public class UserDetailsDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public System.Collections.Generic.List<string> Roles { get; set; } = new();
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDetailsDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.UserStatus == SmartAttendance.Domain.Enums.UserStatus.Active))
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(s.Roles, r => r.Name))));
    }
}

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserDetailsDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(x => x.Roles).AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user == null) return Result<UserDetailsDto>.Failure(new[] { "User not found." });
        var dto = _mapper.Map<UserDetailsDto>(user);
        return Result<UserDetailsDto>.Success(dto);
    }
}


