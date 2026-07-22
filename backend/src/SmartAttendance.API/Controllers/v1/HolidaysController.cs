using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartAttendance.Application.Features.Holidays.Commands;
using SmartAttendance.Application.Features.Holidays.Queries;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class HolidaysController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:Holidays.View")]
    [OutputCache(PolicyName = "StandardCache")]
    public async Task<IActionResult> GetHolidays([FromQuery] GetHolidaysQuery query)
    {
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Holidays.View")]
    public async Task<IActionResult> GetHolidayById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetHolidayByIdQuery { HolidayId = id }));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Holidays.Create")]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Holidays.Update")]
    public async Task<IActionResult> UpdateHoliday(Guid id, [FromBody] UpdateHolidayCommand command)
    {
        if (id != command.HolidayId) return BadRequest();
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Holidays.Delete")]
    public async Task<IActionResult> DeleteHoliday(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteHolidayCommand { HolidayId = id }));
    }
}

