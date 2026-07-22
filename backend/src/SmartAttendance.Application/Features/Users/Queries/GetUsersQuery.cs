using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Queries;

public class GetUsersQuery : PaginationRequest, IRequest<Result<PagedResult<UserDetailsDto>>>
{
}

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDetailsDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

        public async Task<Result<PagedResult<UserDetailsDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
        var count = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(x => x.Username).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var dtos = _mapper.Map<System.Collections.Generic.List<UserDetailsDto>>(users);
        var result = new PagedResult<UserDetailsDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse 
            {
                TotalCount = count,
                PageNumber = request.PageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)Math.Ceiling(count / (double)request.PageSize)
            }
        };
        return Result<PagedResult<UserDetailsDto>>.Success(result);
    }
}


