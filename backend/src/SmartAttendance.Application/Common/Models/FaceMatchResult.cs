namespace SmartAttendance.Application.Common.Models;

// ✅ يعيش في Application (لا Infrastructure) لأن IFaceRecognitionService و VerifyFaceCommand
// في طبقة Application يحتاجون هذا النوع، ولا يجوز لـ Application أن يعتمد على Infrastructure
public class FaceMatchResult
{
    public bool IsMatch { get; set; }
    public double Distance { get; set; }
    public double Confidence { get; set; } // نسبة تقريبية 0-100%
}