using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.Notification.Providers;

public interface INotificationProvider
{
    Task<Result> SendNotificationAsync(string userId, string title, string message);
}
