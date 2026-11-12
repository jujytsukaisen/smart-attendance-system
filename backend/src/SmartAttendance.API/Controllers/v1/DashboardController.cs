using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Dashboard.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    /// <summary>
    /// Returns simplified dashboard data for the current logged-in employee (self-service).
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyDashboard()
    {
        return HandleResult(await Mediator.Send(new GetMyDashboardQuery()));
    }

    [HttpGet]
    [Authorize(Policy = "Permission:Dashboard.View")]
    public async Task<IActionResult> GetDashboard()
    {
        return HandleResult(await Mediator.Send(new GetDashboardStatisticsQuery()));
    }

    // الفرونت إند يستدعي هذا المسار أيضًا - نبقيه كـ alias لنفس البيانات
    [HttpGet("dashboard-statistics")]
    [Authorize(Policy = "Permission:Dashboard.View")]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        return HandleResult(await Mediator.Send(new GetDashboardStatisticsQuery()));
    }
}