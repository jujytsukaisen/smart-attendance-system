using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Companies.DTOs;

namespace SmartAttendance.Application.Features.Companies.Queries;

public class GetCompanyByIdQuery : IRequest<Result<CompanyDetailsDto>>
{
    public Guid CompanyId { get; set; }
}

public class GetCompanyByIdQueryValidator : AbstractValidator<GetCompanyByIdQuery>
{
    public GetCompanyByIdQueryValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCompanyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CompanyDetailsDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == request.CompanyId)
            .ProjectTo<CompanyDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            return Result<CompanyDetailsDto>.Failure(new[] { "Company not found." });
        }

        return Result<CompanyDetailsDto>.Success(dto);
    }
}
