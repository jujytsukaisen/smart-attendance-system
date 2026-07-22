using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.Notification.Providers;

public class NullNotificationProvider : INotificationProvider
{
    public Task<Result> SendNotificationAsync(string userId, string title, string message)
    {
        return Task.FromResult(Result.Failure(new[] { "Notification provider is not configured for production." }));
    }
}
