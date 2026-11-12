using System;

namespace SmartAttendance.Domain.Entities;

/// <summary>
/// Key-Value store for tenant-scoped application settings.
/// Each setting belongs to a specific company (tenant) and is grouped by category.
/// </summary>
public class AppSetting : BaseTenantEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

#pragma warning disable CS8618
    private AppSetting() { }
#pragma warning restore CS8618

    private AppSetting(Guid companyId, string category, string key, string value)
    {
        CompanyId = companyId;
        Category = category;
        Key = key;
        Value = value;
    }

    public static AppSetting Create(Guid companyId, string category, string key, string value)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty.", nameof(category));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty.", nameof(key));

        return new AppSetting(companyId, category, key, value ?? string.Empty);
    }

    public void UpdateValue(string value)
    {
        Value = value ?? string.Empty;
    }
}
