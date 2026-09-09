using System;
using System.Text.Json;
using System.Threading.Tasks;
using RoutineKeeper.Models;

namespace RoutineKeeper.Services;

public class AgentActionExecutor
{
    private readonly LocalDatabaseService _db;
    private readonly NotificationService _notificationService;

    public AgentActionExecutor(LocalDatabaseService db, NotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task ExecuteActionsAsync(string jsonString)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            if (root.TryGetProperty("actions", out var actionsElement) && actionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var action in actionsElement.EnumerateArray())
                {
                    if (action.TryGetProperty("type", out var typeElement) &&
                        action.TryGetProperty("payload", out var payloadElement))
                    {
                        var type = typeElement.GetString();
                        if (type == "add_activity")
                        {
                            await HandleAddActivity(payloadElement);
                        }
                        // Add more actions here in the future
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing agent actions: {ex.Message}");
        }
    }

    private async Task HandleAddActivity(JsonElement payload)
    {
        try
        {
            var title = payload.GetProperty("title").GetString() ?? "New Activity";
            var dateStr = payload.GetProperty("date").GetString();
            var startTimeStr = payload.GetProperty("startTime").GetString();
            var endTimeStr = payload.GetProperty("endTime").GetString();

            if (DateTime.TryParse(dateStr, out var date) &&
                TimeSpan.TryParse(startTimeStr, out var startTime) &&
                TimeSpan.TryParse(endTimeStr, out var endTime))
            {
                var activity = new ActivityItem
                {
                    Name = title,
                    Date = date,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsFlexible = true
                };

                await _db.SaveActivityAsync(activity);

                // Schedule push notification
                _notificationService.ScheduleNotificationForActivity(activity);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing add_activity payload: {ex.Message}");
        }
    }
}
