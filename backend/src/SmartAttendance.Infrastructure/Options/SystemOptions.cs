namespace SmartAttendance.Infrastructure.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

public class EmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class SmsOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class StorageOptions
{
    public string LocalStoragePath { get; set; } = string.Empty;
    public string AzureBlobConnectionString { get; set; } = string.Empty;
    public string S3BucketName { get; set; } = string.Empty;
}

public class FaceRecognitionOptions
{
    // ⚠️ عتبة المسافة الإقليدية بين بصمتين (face-api.js): كل ما قلّت القيمة زادت الدقة
    // القيمة الشائعة والموصى بها: 0.6
    public double RecognitionThreshold { get; set; } = 0.6;

    [Obsolete("استُبدلت بـ RecognitionThreshold لدعم مقارنة البصمات المحلية")]
    public double ConfidenceThreshold { get; set; } = 0.85;
    public string ModelPath { get; set; } = string.Empty;
}

public class GeoLocationOptions
{
    public double DefaultRadiusMeters { get; set; } = 50.0;
    public double MinimumAccuracyMeters { get; set; } = 20.0;
}