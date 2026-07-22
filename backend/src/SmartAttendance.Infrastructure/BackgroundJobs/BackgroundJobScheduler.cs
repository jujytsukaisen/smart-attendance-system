using System;

namespace SmartAttendance.Infrastructure.BackgroundJobs;

public interface IBackgroundJobScheduler
{
    void EnqueueAttendanceCalculation(Guid employeeId, DateTime date);
    void EnqueueNotification(Guid notificationId);
    void ScheduleLeaveExpirationJob();
    void ScheduleHolidaySynchronization();
    void ScheduleDailySummaries();
}

public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    public void EnqueueAttendanceCalculation(Guid employeeId, DateTime date)
    {
        // Abstraction for e.g. BackgroundJob.Enqueue<IAttendanceJob>(x => x.CalculateAsync(...))
    }

    public void EnqueueNotification(Guid notificationId)
    {
        // Abstraction for Notification queue
    }

    public void ScheduleLeaveExpirationJob()
    {
        // Abstraction for RecurringJob
    }

    public void ScheduleHolidaySynchronization()
    {
        // Abstraction for RecurringJob
    }

    public void ScheduleDailySummaries()
    {
        // Abstraction for RecurringJob
    }
}
