using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class RegisterFaceCommand : IRequest<Result<Guid>>
{
    public Guid EmployeeId { get; set; }

    // ⚠️ 5 بصمات وجه — كل بصمة مصفوفة 128 رقم فلوت، مستخرجة من face-api.js في المتصفح
    public float[][] FaceEncodings { get; set; } = Array.Empty<float[]>();
}

public class RegisterFaceCommandValidator : AbstractValidator<RegisterFaceCommand>
{
    public RegisterFaceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.FaceEncodings)
            .NotEmpty()
            .Must(e => e.Length >= 1 && e.Length <= 5)
            .WithMessage("FaceEncodings must contain between 1 and 5 descriptors.")
            .Must(e => e.All(d => d.Length == 128))
            .WithMessage("Each face encoding must contain exactly 128 values (face-api.js descriptor length).");
    }
}

public class RegisterFaceCommandHandler : IRequestHandler<RegisterFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public RegisterFaceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RegisterFaceCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
        if (employee == null) return Result<Guid>.Failure(new[] { "Employee not found." });

        // حساب المتوسط (Average Descriptor) من جميع البصمات للحصول على بصمة أكثر دقة
        var averageEncoding = ComputeAverageEncoding(request.FaceEncodings);
        var encodingJson = JsonSerializer.Serialize(averageEncoding);

        // إذا كان عنده ملف وجه سابق، حدّث البصمة بدل إنشاء ملف جديد
        var existingProfile = await _context.FaceProfiles
            .FirstOrDefaultAsync(p => p.EmployeeId == request.EmployeeId, cancellationToken);

        if (existingProfile is not null)
        {
            existingProfile.SetEncoding(encodingJson);
            return Result<Guid>.Success(existingProfile.Id);
        }

        var profile = SmartAttendance.Domain.Entities.FaceProfile.Create(employee.CompanyId, request.EmployeeId);

        // محليًا لا نحتاج ID خارجي حقيقي، نولّد معرّف داخلي فقط للحفاظ على بنية Register() الحالية
        var localProfileId = Guid.NewGuid().ToString();
        profile.Register(localProfileId);
        profile.SetEncoding(encodingJson);

        await _context.FaceProfiles.AddAsync(profile, cancellationToken);

        return Result<Guid>.Success(profile.Id);
    }

    /// <summary>
    /// يحسب المتوسط الرياضي لمصفوفة من بصمات الوجه (128 رقم لكل بصمة).
    /// البصمة المتوسطة أكثر دقة لأنها تجمع خصائص الوجه من عدة زوايا.
    /// </summary>
    private static float[] ComputeAverageEncoding(float[][] encodings)
    {
        if (encodings.Length == 1) return encodings[0];

        var length = encodings[0].Length; // 128
        var average = new float[length];

        for (var i = 0; i < length; i++)
        {
            average[i] = encodings.Sum(e => e[i]) / encodings.Length;
        }

        return average;
    }
}