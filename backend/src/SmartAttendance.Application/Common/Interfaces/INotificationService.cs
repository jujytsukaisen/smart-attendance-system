using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Interfaces;

public interface INotificationService
{
    Task<Result> SendNotificationAsync(string userId, string title, string message);
}
