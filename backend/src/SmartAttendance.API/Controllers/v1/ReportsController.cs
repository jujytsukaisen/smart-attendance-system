using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Reports.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class ReportsController : ApiControllerBase
{
    [HttpGet("dashboard-statistics")]
    [Authorize(Policy = "Permission:Reports.View")]
    public async Task<IActionResult> GetDashboardStatistics([FromQuery] GetReportsDashboardStatisticsQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("late-employees")]
    [Authorize(Policy = "Permission:Reports.View")]
    public async Task<IActionResult> GetLateEmployees([FromQuery] GetLateEmployeesReportQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("absence")]
    [Authorize(Policy = "Permission:Reports.View")]
    public async Task<IActionResult> GetAbsenceReport([FromQuery] GetAbsenceReportQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("leave")]
    [Authorize(Policy = "Permission:Reports.View")]
    public async Task<IActionResult> GetLeaveReport([FromQuery] GetLeaveReportQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost("export")]
    [Authorize(Policy = "Permission:Reports.Export")]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportsQuery query)
    {
        var result = await Mediator.Send(query);
        if (!result.Succeeded || result.Data == null)
            return ProblemDetailsForFailure(result);

        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    [HttpGet("export")]
    [Authorize(Policy = "Permission:Reports.Export")]
    public async Task<IActionResult> ExportReportGet([FromQuery] ExportReportsQuery query)
    {
        var result = await Mediator.Send(query);
        if (!result.Succeeded || result.Data == null)
            return ProblemDetailsForFailure(result);

        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }
}