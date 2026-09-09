using System;
using System.Threading.Tasks;
using RoutineKeeper.Models;

#if WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
#endif

namespace RoutineKeeper.Services;

public class NotificationService
{
    public void ScheduleNotificationForActivity(ActivityItem activity)
    {
        var notificationTime = activity.Date.Date + activity.StartTime;
        var delay = notificationTime - DateTime.Now;

        // Don't schedule if time has already passed
        if (delay.TotalMilliseconds <= 0) return;

        // In-memory scheduler for when the app is running
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay);
            ShowToast($"Activity Starting: {activity.Name}", $"Your scheduled block '{activity.Name}' is starting now.");
        });
    }

    private void ShowToast(string title, string message)
    {
#if WINDOWS
        try
        {
            var appNotification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message)
                .BuildNotification();

            AppNotificationManager.Default.Show(appNotification);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to show Windows Toast: {ex.Message}");
        }
#endif
    }
}
