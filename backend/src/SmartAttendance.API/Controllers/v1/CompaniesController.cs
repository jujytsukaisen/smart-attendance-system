using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartAttendance.Application.Features.Companies.Commands;
using SmartAttendance.Application.Features.Companies.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class CompaniesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Companies.View")]
    [OutputCache(PolicyName = "StandardCache")]
    public async Task<IActionResult> GetCompanies([FromQuery] GetCompaniesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Companies.View")]
    public async Task<IActionResult> GetCompanyById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetCompanyByIdQuery { CompanyId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Companies.Create")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Companies.Update")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyCommand command)
    {
        if (id != command.CompanyId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Companies.Delete")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteCompanyCommand { CompanyId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Companies.Restore")]
    public async Task<IActionResult> RestoreCompany(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreCompanyCommand { CompanyId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:Companies.Update")]
    public async Task<IActionResult> ActivateCompany(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateCompanyCommand { CompanyId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:Companies.Update")]
    public async Task<IActionResult> DeactivateCompany(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateCompanyCommand { CompanyId = id }));
    }
}
