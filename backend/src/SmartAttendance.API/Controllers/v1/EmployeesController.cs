using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Employees.Commands;
using SmartAttendance.Application.Features.Employees.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class EmployeesController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Employees.View")]
    public async Task<IActionResult> GetEmployees([FromQuery] GetEmployeesQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("company/{companyId}")]
    [Authorize(Policy = "Permission:Employees.View")]
    public async Task<IActionResult> GetEmployeesByCompany(Guid companyId, [FromQuery] GetEmployeesByCompanyQuery query)
    {
        query.CompanyId = companyId;
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("branch/{branchId}")]
    [Authorize(Policy = "Permission:Employees.View")]
    public async Task<IActionResult> GetEmployeesByBranch(Guid branchId, [FromQuery] GetEmployeesByBranchQuery query)
    {
        query.BranchId = branchId;
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("department/{departmentId}")]
    [Authorize(Policy = "Permission:Employees.View")]
    public async Task<IActionResult> GetEmployeesByDepartment(Guid departmentId, [FromQuery] GetEmployeesByDepartmentQuery query)
    {
        query.DepartmentId = departmentId;
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Employees.View")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetEmployeeByIdQuery { EmployeeId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Employees.Create")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Employees.Update")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeCommand command)
    {
        if (id != command.EmployeeId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Employees.Delete")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteEmployeeCommand { EmployeeId = id }));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Employees.Update")] // Update or Manage permission depending on setup, but typically restoring is an update action
    public async Task<IActionResult> RestoreEmployee(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreEmployeeCommand { EmployeeId = id }));
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "Permission:Employees.Update")]
    public async Task<IActionResult> ActivateEmployee(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateEmployeeCommand { EmployeeId = id }));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "Permission:Employees.Update")]
    public async Task<IActionResult> DeactivateEmployee(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateEmployeeCommand { EmployeeId = id }));
    }
}
