using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Settings.Commands;

/// <summary>
/// Shared helper for Settings upsert logic.
/// Loads existing settings for a category, updates existing entries, and adds missing ones.
/// This avoids IQueryable + in-memory list mixing (lesson learned).
/// </summary>
internal static class SettingsHelper
{
    public static async Task UpsertAsync(
        IApplicationDbContext context,
        Guid companyId,
        string category,
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        // Step 1: Load existing settings for this category into memory first
        var existing = await context.AppSettings
            .Where(s => s.CompanyId == companyId && s.Category == category)
            .ToListAsync(cancellationToken);

        // Step 2: Process in memory (no IQueryable mixing)
        foreach (var kv in values)
        {
            var setting = existing.FirstOrDefault(s => s.Key == kv.Key);
            if (setting != null)
            {
                setting.UpdateValue(kv.Value);
                context.AppSettings.Update(setting);
            }
            else
            {
                var newSetting = AppSetting.Create(companyId, category, kv.Key, kv.Value);
                await context.AppSettings.AddAsync(newSetting, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task<Dictionary<string, string>> GetCategoryAsync(
        IApplicationDbContext context,
        Guid companyId,
        string category,
        CancellationToken cancellationToken)
    {
        var settings = await context.AppSettings
            .Where(s => s.CompanyId == companyId && s.Category == category)
            .ToListAsync(cancellationToken);

        return settings.ToDictionary(s => s.Key, s => s.Value);
    }

    public static string Get(Dictionary<string, string> dict, string key, string defaultValue = "")
        => dict.TryGetValue(key, out var val) ? val : defaultValue;

    public static bool GetBool(Dictionary<string, string> dict, string key, bool defaultValue = false)
        => bool.TryParse(Get(dict, key, defaultValue.ToString()), out var v) ? v : defaultValue;

    public static int GetInt(Dictionary<string, string> dict, string key, int defaultValue = 0)
        => int.TryParse(Get(dict, key, defaultValue.ToString()), out var v) ? v : defaultValue;

    public static double GetDouble(Dictionary<string, string> dict, string key, double defaultValue = 0)
        => double.TryParse(Get(dict, key, defaultValue.ToString()), out var v) ? v : defaultValue;
}
