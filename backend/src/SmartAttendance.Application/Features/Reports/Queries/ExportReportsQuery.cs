using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Reports.DTOs;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Reports.Queries;

public class ExportReportsQuery : IRequest<Result<ExportFileDto>>
{
    public string Type { get; set; } = "csv"; // "pdf", "csv", "excel"
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class ExportReportsQueryHandler : IRequestHandler<ExportReportsQuery, Result<ExportFileDto>>
{
    private readonly IApplicationDbContext _context;

    public ExportReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExportFileDto>> Handle(ExportReportsQuery request, CancellationToken cancellationToken)
    {
        var startDate = (request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))).ToDateTime(TimeOnly.MinValue);
        var endDate = (request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToDateTime(TimeOnly.MaxValue);

        // 1. Employees
        var empQuery = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Branch)
            .Where(e => e.EmployeeStatus == EmployeeStatus.Active)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            empQuery = empQuery.Where(e => e.DepartmentId == request.DepartmentId.Value);
        if (request.BranchId.HasValue)
            empQuery = empQuery.Where(e => e.BranchId == request.BranchId.Value);
        if (request.EmployeeId.HasValue)
            empQuery = empQuery.Where(e => e.Id == request.EmployeeId.Value);

        var employees = await empQuery.ToListAsync(cancellationToken);
        var employeeIds = employees.Select(e => e.Id).ToHashSet();
        var totalEmployees = employees.Count;

        // 2. Attendances in range
        var attQuery = _context.Attendances
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .Where(a => a.CheckInTime >= startDate && a.CheckInTime <= endDate)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            attQuery = attQuery.Where(a => a.Employee.DepartmentId == request.DepartmentId.Value);
        if (request.BranchId.HasValue)
            attQuery = attQuery.Where(a => a.Employee.BranchId == request.BranchId.Value);
        if (request.EmployeeId.HasValue)
            attQuery = attQuery.Where(a => a.EmployeeId == request.EmployeeId.Value);

        var attendances = await attQuery
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync(cancellationToken);

        // 3. Today metrics
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);
        var todayAttendances = attendances
            .Where(a => a.CheckInTime >= todayStart && a.CheckInTime <= todayEnd)
            .ToList();

        var presentToday = todayAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave);
        var lateToday = todayAttendances.Count(a => a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave);
        var absentToday = Math.Max(0, totalEmployees - todayAttendances.Count);

        // 4. Leaves
        var leaveQuery = _context.Leaves
            .Include(l => l.Employee).ThenInclude(e => e.Department)
            .Where(l => l.StartDate <= endDate && l.EndDate >= startDate)
            .AsQueryable();

        if (request.EmployeeId.HasValue)
            leaveQuery = leaveQuery.Where(l => l.EmployeeId == request.EmployeeId.Value);
        else if (request.DepartmentId.HasValue)
            leaveQuery = leaveQuery.Where(l => l.Employee.DepartmentId == request.DepartmentId.Value);

        var leaves = await leaveQuery
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(cancellationToken);

        var onLeaveToday = leaves.Count(l => l.LeaveStatus == LeaveStatus.Approved && l.StartDate <= DateTime.UtcNow && l.EndDate >= DateTime.UtcNow);
        var avgRate = totalEmployees > 0 ? Math.Round((double)(presentToday + lateToday) / totalEmployees * 100, 1) : 0;

        // 5. Late employees summary
        var lateSummary = attendances
            .Where(a => a.AttendanceStatus == AttendanceStatus.Late)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new
            {
                EmployeeName = $"{g.First().Employee.FirstName} {g.First().Employee.LastName}",
                Department = g.First().Employee.Department?.Name ?? "-",
                TotalLateMinutes = g.Sum(a => a.LateMinutes),
                LateCount = g.Count()
            })
            .OrderByDescending(x => x.TotalLateMinutes)
            .ToList();

        var exportType = (request.Type ?? "csv").Trim().ToLowerInvariant();
        var dateStamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        if (exportType == "csv")
        {
            var content = GenerateCsv(totalEmployees, presentToday, absentToday, lateToday, onLeaveToday, avgRate, attendances, lateSummary, leaves);
            var preamble = Encoding.UTF8.GetPreamble();
            var body = Encoding.UTF8.GetBytes(content);
            var fullBytes = new byte[preamble.Length + body.Length];
            Buffer.BlockCopy(preamble, 0, fullBytes, 0, preamble.Length);
            Buffer.BlockCopy(body, 0, fullBytes, preamble.Length, body.Length);

            return Result<ExportFileDto>.Success(new ExportFileDto
            {
                Content = fullBytes,
                ContentType = "text/csv; charset=utf-8",
                FileName = $"Attendance_Report_{dateStamp}.csv"
            });
        }
        else if (exportType == "excel")
        {
            var content = GenerateExcelXml(totalEmployees, presentToday, absentToday, lateToday, onLeaveToday, avgRate, attendances, lateSummary, leaves);
            var bytes = Encoding.UTF8.GetBytes(content);

            return Result<ExportFileDto>.Success(new ExportFileDto
            {
                Content = bytes,
                ContentType = "application/vnd.ms-excel; charset=utf-8",
                FileName = $"Attendance_Report_{dateStamp}.xls"
            });
        }
        else // pdf / html
        {
            var content = GenerateHtmlPrintable(totalEmployees, presentToday, absentToday, lateToday, onLeaveToday, avgRate, attendances, lateSummary, leaves, startDate, endDate);
            var bytes = Encoding.UTF8.GetBytes(content);

            return Result<ExportFileDto>.Success(new ExportFileDto
            {
                Content = bytes,
                ContentType = "text/html; charset=utf-8",
                FileName = $"Attendance_Report_{dateStamp}.html"
            });
        }
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }

    private static string GenerateCsv(
        int totalEmployees, int presentToday, int absentToday, int lateToday, int onLeaveToday, double avgRate,
        List<Domain.Entities.Attendance> attendances,
        dynamic lateSummary,
        List<Domain.Entities.Leave> leaves)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Attendance Summary Report / تقرير ملخص الحضور ===");
        sb.AppendLine($"Generated At / تاريخ التوليد,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"Total Employees / إجمالي الموظفين,{totalEmployees}");
        sb.AppendLine($"Present Today / الحاضرون اليوم,{presentToday}");
        sb.AppendLine($"Absent Today / الغائبون اليوم,{absentToday}");
        sb.AppendLine($"Late Today / المتأخرون اليوم,{lateToday}");
        sb.AppendLine($"On Leave Today / في إجازة اليوم,{onLeaveToday}");
        sb.AppendLine($"Attendance Rate / معدل الحضور,{avgRate}%");
        sb.AppendLine();

        sb.AppendLine("=== Detailed Attendance Records / سجلات الحضور التفصيلية ===");
        sb.AppendLine("Date,Employee Name,Department,Check-In,Check-Out,Working (Min),Late (Min),Overtime (Min),Excess Break (Min),Status");
        foreach (var a in attendances)
        {
            var empName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "-";
            var dept = a.Employee?.Department?.Name ?? "-";
            var checkIn = a.CheckInTime.ToString("yyyy-MM-dd hh:mm tt");
            var checkOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("yyyy-MM-dd hh:mm tt") : "-";
            sb.AppendLine($"\"{a.CheckInTime:yyyy-MM-dd}\",\"{EscapeCsv(empName)}\",\"{EscapeCsv(dept)}\",\"{checkIn}\",\"{checkOut}\",{a.WorkingMinutes},{a.LateMinutes},{a.OvertimeMinutes},{a.ExcessBreakMinutes},\"{a.AttendanceStatus}\"");
        }
        sb.AppendLine();

        sb.AppendLine("=== Late Arrivals Summary / ملخص التأخير ===");
        sb.AppendLine("Employee Name,Department,Total Late Minutes,Incidents Count");
        foreach (var l in lateSummary)
        {
            sb.AppendLine($"\"{EscapeCsv(l.EmployeeName)}\",\"{EscapeCsv(l.Department)}\",{l.TotalLateMinutes},{l.LateCount}");
        }
        sb.AppendLine();

        sb.AppendLine("=== Leaves Log / سجل الإجازات ===");
        sb.AppendLine("Employee Name,Leave Type,Start Date,End Date,Total Days,Status");
        foreach (var lv in leaves)
        {
            var empName = lv.Employee != null ? $"{lv.Employee.FirstName} {lv.Employee.LastName}" : "-";
            sb.AppendLine($"\"{EscapeCsv(empName)}\",\"{lv.LeaveType}\",\"{lv.StartDate:yyyy-MM-dd}\",\"{lv.EndDate:yyyy-MM-dd}\",{lv.TotalDays},\"{lv.LeaveStatus}\"");
        }

        return sb.ToString();
    }

    private static string GenerateExcelXml(
        int totalEmployees, int presentToday, int absentToday, int lateToday, int onLeaveToday, double avgRate,
        List<Domain.Entities.Attendance> attendances,
        dynamic lateSummary,
        List<Domain.Entities.Leave> leaves)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
        sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");

        // Styles
        sb.AppendLine(" <Styles>");
        sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\"><Alignment ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\"/></Style>");
        sb.AppendLine("  <Style ss:ID=\"Header\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"11\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#2563EB\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/></Style>");
        sb.AppendLine("  <Style ss:ID=\"Title\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"14\" ss:Bold=\"1\" ss:Color=\"#1E293B\"/><Alignment ss:Vertical=\"Center\"/></Style>");
        sb.AppendLine("  <Style ss:ID=\"StatLabel\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#475569\"/><Interior ss:Color=\"#F1F5F9\" ss:Pattern=\"Solid\"/></Style>");
        sb.AppendLine("  <Style ss:ID=\"StatVal\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#0F172A\"/></Style>");
        sb.AppendLine(" </Styles>");

        // Sheet 1: Summary & Attendance
        sb.AppendLine(" <Worksheet ss:Name=\"Attendance Logs\">");
        sb.AppendLine("  <Table>");
        sb.AppendLine("   <Column ss:Width=\"90\"/>");
        sb.AppendLine("   <Column ss:Width=\"140\"/>");
        sb.AppendLine("   <Column ss:Width=\"120\"/>");
        sb.AppendLine("   <Column ss:Width=\"110\"/>");
        sb.AppendLine("   <Column ss:Width=\"110\"/>");
        sb.AppendLine("   <Column ss:Width=\"80\"/>");
        sb.AppendLine("   <Column ss:Width=\"70\"/>");
        sb.AppendLine("   <Column ss:Width=\"80\"/>");
        sb.AppendLine("   <Column ss:Width=\"90\"/>");
        sb.AppendLine("   <Column ss:Width=\"90\"/>");

        sb.AppendLine("   <Row ss:Height=\"28\"><Cell ss:StyleID=\"Title\"><Data ss:Type=\"String\">Attendance Summary Report</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell ss:StyleID=\"StatLabel\"><Data ss:Type=\"String\">Total Employees</Data></Cell><Cell ss:StyleID=\"StatVal\"><Data ss:Type=\"Number\">{totalEmployees}</Data></Cell><Cell ss:StyleID=\"StatLabel\"><Data ss:Type=\"String\">Present Today</Data></Cell><Cell ss:StyleID=\"StatVal\"><Data ss:Type=\"Number\">{presentToday}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell ss:StyleID=\"StatLabel\"><Data ss:Type=\"String\">Late Today</Data></Cell><Cell ss:StyleID=\"StatVal\"><Data ss:Type=\"Number\">{lateToday}</Data></Cell><Cell ss:StyleID=\"StatLabel\"><Data ss:Type=\"String\">Absent Today</Data></Cell><Cell ss:StyleID=\"StatVal\"><Data ss:Type=\"Number\">{absentToday}</Data></Cell></Row>");
        sb.AppendLine($"   <Row><Cell ss:StyleID=\"StatLabel\"><Data ss:Type=\"String\">On Leave Today</Data></Cell><Cell ss:StyleID=\"StatVal\"><Data ss:Type=\"Number\">{onLeaveToday}</Data></Cell><Cell ss:StyleID=\"StatLabel\"><Data ss:Type=\"String\">Attendance Rate</Data></Cell><Cell ss:StyleID=\"StatVal\"><Data ss:Type=\"String\">{avgRate}%</Data></Cell></Row>");
        sb.AppendLine("   <Row/>");

        // Headers
        sb.AppendLine("   <Row ss:Height=\"22\">");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Date</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Employee</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Department</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Check-In</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Check-Out</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Working (Min)</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Late (Min)</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Overtime (Min)</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Excess Break (Min)</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Status</Data></Cell>");
        sb.AppendLine("   </Row>");

        foreach (var a in attendances)
        {
            var empName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "-";
            var dept = a.Employee?.Department?.Name ?? "-";
            var checkIn = a.CheckInTime.ToString("yyyy-MM-dd hh:mm tt");
            var checkOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("yyyy-MM-dd hh:mm tt") : "-";

            sb.AppendLine("   <Row>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{a.CheckInTime:yyyy-MM-dd}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{EscapeXml(empName)}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{EscapeXml(dept)}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{checkIn}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{checkOut}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{a.WorkingMinutes}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{a.LateMinutes}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{a.OvertimeMinutes}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{a.ExcessBreakMinutes}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{a.AttendanceStatus}</Data></Cell>");
            sb.AppendLine("   </Row>");
        }

        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");

        // Sheet 2: Late Summary
        sb.AppendLine(" <Worksheet ss:Name=\"Late Summary\">");
        sb.AppendLine("  <Table>");
        sb.AppendLine("   <Column ss:Width=\"150\"/>");
        sb.AppendLine("   <Column ss:Width=\"130\"/>");
        sb.AppendLine("   <Column ss:Width=\"120\"/>");
        sb.AppendLine("   <Column ss:Width=\"100\"/>");
        sb.AppendLine("   <Row ss:Height=\"22\">");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Employee</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Department</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Total Late Minutes</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Incidents</Data></Cell>");
        sb.AppendLine("   </Row>");
        foreach (var l in lateSummary)
        {
            sb.AppendLine("   <Row>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{EscapeXml(l.EmployeeName)}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{EscapeXml(l.Department)}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{l.TotalLateMinutes}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{l.LateCount}</Data></Cell>");
            sb.AppendLine("   </Row>");
        }
        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");

        // Sheet 3: Leaves
        sb.AppendLine(" <Worksheet ss:Name=\"Leaves Log\">");
        sb.AppendLine("  <Table>");
        sb.AppendLine("   <Column ss:Width=\"150\"/>");
        sb.AppendLine("   <Column ss:Width=\"100\"/>");
        sb.AppendLine("   <Column ss:Width=\"100\"/>");
        sb.AppendLine("   <Column ss:Width=\"100\"/>");
        sb.AppendLine("   <Column ss:Width=\"80\"/>");
        sb.AppendLine("   <Column ss:Width=\"90\"/>");
        sb.AppendLine("   <Row ss:Height=\"22\">");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Employee</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Leave Type</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Start Date</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">End Date</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Total Days</Data></Cell>");
        sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">Status</Data></Cell>");
        sb.AppendLine("   </Row>");
        foreach (var lv in leaves)
        {
            var empName = lv.Employee != null ? $"{lv.Employee.FirstName} {lv.Employee.LastName}" : "-";
            sb.AppendLine("   <Row>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{EscapeXml(empName)}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{lv.LeaveType}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{lv.StartDate:yyyy-MM-dd}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{lv.EndDate:yyyy-MM-dd}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"Number\">{lv.TotalDays}</Data></Cell>");
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{lv.LeaveStatus}</Data></Cell>");
            sb.AppendLine("   </Row>");
        }
        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");

        sb.AppendLine("</Workbook>");
        return sb.ToString();
    }

    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static string GenerateHtmlPrintable(
        int totalEmployees, int presentToday, int absentToday, int lateToday, int onLeaveToday, double avgRate,
        List<Domain.Entities.Attendance> attendances,
        dynamic lateSummary,
        List<Domain.Entities.Leave> leaves,
        DateTime startDate, DateTime endDate)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\" dir=\"ltr\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>Smart Attendance Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');");
        sb.AppendLine("    * { box-sizing: border-box; font-family: 'Inter', 'Segoe UI', Tahoma, sans-serif; }");
        sb.AppendLine("    body { background-color: #f8fafc; color: #1e293b; margin: 0; padding: 24px; }");
        sb.AppendLine("    .container { max-width: 1000px; margin: 0 auto; background: #ffffff; padding: 32px; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }");
        sb.AppendLine("    .header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #e2e8f0; padding-bottom: 20px; margin-bottom: 24px; }");
        sb.AppendLine("    .header h1 { margin: 0; font-size: 24px; color: #0f172a; }");
        sb.AppendLine("    .header p { margin: 4px 0 0; font-size: 13px; color: #64748b; }");
        sb.AppendLine("    .badge { background: #3b82f6; color: #fff; padding: 6px 14px; border-radius: 9999px; font-size: 12px; font-weight: 600; }");
        sb.AppendLine("    .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(140px, 1fr)); gap: 16px; margin-bottom: 28px; }");
        sb.AppendLine("    .stat-card { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px; text-align: center; }");
        sb.AppendLine("    .stat-card .val { font-size: 22px; font-weight: 700; color: #0f172a; margin-top: 4px; }");
        sb.AppendLine("    .stat-card .lbl { font-size: 12px; color: #64748b; font-weight: 600; }");
        sb.AppendLine("    .section-title { font-size: 18px; font-weight: 700; color: #1e293b; margin: 24px 0 12px; border-left: 4px solid #3b82f6; padding-left: 10px; }");
        sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin-bottom: 24px; font-size: 13px; }");
        sb.AppendLine("    th { background: #f1f5f9; color: #334155; font-weight: 600; text-align: start; padding: 10px 12px; border-bottom: 2px solid #cbd5e1; }");
        sb.AppendLine("    td { padding: 9px 12px; border-bottom: 1px solid #e2e8f0; }");
        sb.AppendLine("    tr:nth-child(even) { background-color: #f8fafc; }");
        sb.AppendLine("    .status-present { color: #16a34a; font-weight: 600; }");
        sb.AppendLine("    .status-late { color: #d97706; font-weight: 600; }");
        sb.AppendLine("    .status-absent { color: #dc2626; font-weight: 600; }");
        sb.AppendLine("    .print-btn { position: fixed; bottom: 24px; right: 24px; background: #2563eb; color: #fff; border: none; padding: 12px 24px; border-radius: 8px; font-size: 14px; font-weight: 600; cursor: pointer; box-shadow: 0 4px 12px rgba(37,99,235,0.3); }");
        sb.AppendLine("    .print-btn:hover { background: #1d4ed8; }");
        sb.AppendLine("    @media print {");
        sb.AppendLine("      body { background: #fff; padding: 0; }");
        sb.AppendLine("      .container { box-shadow: none; padding: 0; width: 100%; max-width: 100%; }");
        sb.AppendLine("      .print-btn { display: none; }");
        sb.AppendLine("    }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <button class=\"print-btn\" onclick=\"window.print()\">🖨️ Print / Save PDF</button>");
        sb.AppendLine("  <div class=\"container\">");
        sb.AppendLine("    <div class=\"header\">");
        sb.AppendLine("      <div>");
        sb.AppendLine("        <h1>Comprehensive Attendance Report</h1>");
        sb.AppendLine($"        <p>Period from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} | Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"badge\">Smart Attendance</div>");
        sb.AppendLine("    </div>");

        // Stats Cards
        sb.AppendLine("    <div class=\"stats-grid\">");
        sb.AppendLine($"      <div class=\"stat-card\"><div class=\"lbl\">Total Employees</div><div class=\"val\">{totalEmployees}</div></div>");
        sb.AppendLine($"      <div class=\"stat-card\"><div class=\"lbl\">Present Today</div><div class=\"val\" style=\"color:#16a34a\">{presentToday}</div></div>");
        sb.AppendLine($"      <div class=\"stat-card\"><div class=\"lbl\">Late Today</div><div class=\"val\" style=\"color:#d97706\">{lateToday}</div></div>");
        sb.AppendLine($"      <div class=\"stat-card\"><div class=\"lbl\">Absent Today</div><div class=\"val\" style=\"color:#dc2626\">{absentToday}</div></div>");
        sb.AppendLine($"      <div class=\"stat-card\"><div class=\"lbl\">On Leave</div><div class=\"val\" style=\"color:#0284c7\">{onLeaveToday}</div></div>");
        sb.AppendLine($"      <div class=\"stat-card\"><div class=\"lbl\">Compliance Rate</div><div class=\"val\">{avgRate}%</div></div>");
        sb.AppendLine("    </div>");

        // Attendance Table
        sb.AppendLine("    <div class=\"section-title\">Attendance Records (Recent)</div>");
        sb.AppendLine("    <table>");
        sb.AppendLine("      <thead><tr><th>Date</th><th>Employee</th><th>Department</th><th>Check-In</th><th>Check-Out</th><th>Working Hours</th><th>Late</th><th>Overtime</th><th>Excess Break</th><th>Status</th></tr></thead>");
        sb.AppendLine("      <tbody>");
        foreach (var a in attendances.Take(100))
        {
            var empName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "-";
            var dept = a.Employee?.Department?.Name ?? "-";
            var checkIn = a.CheckInTime.ToString("hh:mm tt");
            var checkOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("hh:mm tt") : "-";
            var hours = $"{a.WorkingMinutes / 60}h {a.WorkingMinutes % 60}m";
            var overtime = a.OvertimeMinutes > 0 ? $"{a.OvertimeMinutes / 60}h {a.OvertimeMinutes % 60}m" : "-";
            var late = a.LateMinutes > 0 ? $"{a.LateMinutes / 60}h {a.LateMinutes % 60}m" : "-";
            var excessBreak = a.ExcessBreakMinutes > 0 ? $"{a.ExcessBreakMinutes / 60}h {a.ExcessBreakMinutes % 60}m" : "-";
            var statusClass = a.AttendanceStatus == AttendanceStatus.Present || a.AttendanceStatus == AttendanceStatus.PresentDuringLeave ? "status-present" : (a.AttendanceStatus == AttendanceStatus.Late || a.AttendanceStatus == AttendanceStatus.LateDuringLeave ? "status-late" : "status-absent");

            sb.AppendLine($"        <tr><td>{a.CheckInTime:yyyy-MM-dd}</td><td><strong>{EscapeXml(empName)}</strong></td><td>{EscapeXml(dept)}</td><td>{checkIn}</td><td>{checkOut}</td><td>{hours}</td><td>{late}</td><td>{overtime}</td><td>{excessBreak}</td><td class=\"{statusClass}\">{a.AttendanceStatus}</td></tr>");
        }
        sb.AppendLine("      </tbody>");
        sb.AppendLine("    </table>");

        // Late Summary
        if (lateSummary.Count > 0)
        {
            sb.AppendLine("    <div class=\"section-title\">Most Late Employees Summary</div>");
            sb.AppendLine("    <table>");
            sb.AppendLine("      <thead><tr><th>Employee</th><th>Department</th><th>Total Late Time</th><th>Incidents</th></tr></thead>");
            sb.AppendLine("      <tbody>");
            foreach (var l in lateSummary)
            {
                var totalLateStr = l.TotalLateMinutes > 0 ? $"{l.TotalLateMinutes / 60}h {l.TotalLateMinutes % 60}m" : "-";
                sb.AppendLine($"        <tr><td><strong>{EscapeXml(l.EmployeeName)}</strong></td><td>{EscapeXml(l.Department)}</td><td style=\"color:#d97706;font-weight:600;\">{totalLateStr}</td><td>{l.LateCount} times</td></tr>");
            }
            sb.AppendLine("      </tbody>");
            sb.AppendLine("    </table>");
        }

        // Leaves Summary
        if (leaves.Count > 0)
        {
            sb.AppendLine("    <div class=\"section-title\">Leaves Log</div>");
            sb.AppendLine("    <table>");
            sb.AppendLine("      <thead><tr><th>Employee</th><th>Leave Type</th><th>From</th><th>To</th><th>Days</th><th>Status</th></tr></thead>");
            sb.AppendLine("      <tbody>");
            foreach (var lv in leaves)
            {
                var empName = lv.Employee != null ? $"{lv.Employee.FirstName} {lv.Employee.LastName}" : "-";
                sb.AppendLine($"        <tr><td><strong>{EscapeXml(empName)}</strong></td><td>{lv.LeaveType}</td><td>{lv.StartDate:yyyy-MM-dd}</td><td>{lv.EndDate:yyyy-MM-dd}</td><td>{lv.TotalDays}</td><td>{lv.LeaveStatus}</td></tr>");
            }
            sb.AppendLine("      </tbody>");
            sb.AppendLine("    </table>");
        }

        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }
}
