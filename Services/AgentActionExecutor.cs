using System;
using System.Text.Json;
using System.Threading.Tasks;
using RoutineKeeper.Models;
using CommunityToolkit.Mvvm.Messaging;
using RoutineKeeper.Messages;

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

    public async Task<string> ExecuteSpecificActionAsync(string intent, string jsonPayload)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonPayload);
            var root = document.RootElement;

            string resultMsg = "Unknown intent.";
            bool didMutate = false;

            if (intent == "ADD_ACTIVITY")
            {
                await HandleAddActivity(root);
                resultMsg = "Activity added successfully.";
                didMutate = true;
            }
            else if (intent == "ADD_RECURRING_ACTIVITY")
            {
                int added = await HandleAddRecurringActivity(root);
                if (added == 0)
                {
                    throw new Exception("0 activities added. Please specify correct dates and days of the week.");
                }
                resultMsg = $"Recurring activity added successfully ({added} days).";
                didMutate = true;
            }
            else if (intent == "UPDATE_ACTIVITY")
            {
                await HandleUpdateActivity(root);
                resultMsg = "Activity updated successfully.";
                didMutate = true;
            }
            else if (intent == "DELETE_ACTIVITY")
            {
                await HandleDeleteActivity(root);
                resultMsg = "Activity deleted successfully.";
                didMutate = true;
            }
            else if (intent == "FETCH_SCHEDULE")
            {
                var result = await HandleFetchSchedule(root);
                resultMsg = result ?? "No schedule found.";
            }
            else if (intent == "CLEAR_SCHEDULE")
            {
                await _db.DeleteAllActivitiesAsync();
                resultMsg = "All activities cleared from the schedule.";
                didMutate = true;
            }

            if (didMutate)
            {
                WeakReferenceMessenger.Default.Send(new ScheduleChangedMessage());
            }

            return resultMsg;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error executing agent action: {ex.Message}");
            return $"Failed to execute {intent}. Error: {ex.Message}. Make sure dates and times are correctly formatted.";
        }
    }

    private async Task HandleAddActivity(JsonElement payload)
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
            _notificationService.ScheduleNotificationForActivity(activity);
        }
        else
        {
            throw new Exception("Invalid date or time format");
        }
    }

    private async Task<int> HandleAddRecurringActivity(JsonElement payload)
    {
        var title = payload.GetProperty("title").GetString() ?? "New Activity";
        var startDateStr = payload.GetProperty("startDate").GetString();
        var endDateStr = payload.GetProperty("endDate").GetString();
        var startTimeStr = payload.GetProperty("startTime").GetString();
        var endTimeStr = payload.GetProperty("endTime").GetString();
        
        var daysOfWeek = new System.Collections.Generic.HashSet<int>();
        if (payload.TryGetProperty("daysOfWeek", out var daysElement) && daysElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var day in daysElement.EnumerateArray())
            {
                if (day.ValueKind == JsonValueKind.Number && day.TryGetInt32(out int dayNum))
                {
                    daysOfWeek.Add(dayNum);
                }
                else if (day.ValueKind == JsonValueKind.String)
                {
                    var dayStr = day.GetString()?.ToLower();
                    if (dayStr != null)
                    {
                        if (dayStr.Contains("niedziel")) daysOfWeek.Add(0);
                        else if (dayStr.Contains("poniedzia")) daysOfWeek.Add(1);
                        else if (dayStr.Contains("wtor")) daysOfWeek.Add(2);
                        else if (dayStr.Contains("środ") || dayStr.Contains("srod")) daysOfWeek.Add(3);
                        else if (dayStr.Contains("czwart")) daysOfWeek.Add(4);
                        else if (dayStr.Contains("piąt") || dayStr.Contains("piat")) daysOfWeek.Add(5);
                        else if (dayStr.Contains("sobot")) daysOfWeek.Add(6);
                    }
                }
            }
        }

        if (DateTime.TryParse(startDateStr, out var startDate) &&
            DateTime.TryParse(endDateStr, out var endDate) &&
            TimeSpan.TryParse(startTimeStr, out var startTime) &&
            TimeSpan.TryParse(endTimeStr, out var endTime))
        {
            if ((endDate - startDate).TotalDays > 366 * 2) 
            {
                endDate = startDate.AddDays(366 * 2); 
            }

            int addedCount = 0;
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                if (daysOfWeek.Contains((int)currentDate.DayOfWeek))
                {
                    var activity = new ActivityItem
                    {
                        Name = title,
                        Date = currentDate,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsFlexible = true
                    };

                    await _db.SaveActivityAsync(activity);
                    _notificationService.ScheduleNotificationForActivity(activity);
                    addedCount++;
                }
                currentDate = currentDate.AddDays(1);
            }
            return addedCount;
        }
        else
        {
            throw new Exception("Invalid date or time format in payload");
        }
    }

    private async Task HandleUpdateActivity(JsonElement payload)
    {
        var id = payload.GetProperty("id").GetInt32();
        var activity = await _db.GetActivityByIdAsync(id);
        if (activity != null)
        {
            if (payload.TryGetProperty("title", out var titleProp))
                activity.Name = titleProp.GetString() ?? activity.Name;

            if (payload.TryGetProperty("startTime", out var startProp) && TimeSpan.TryParse(startProp.GetString(), out var startTime))
                activity.StartTime = startTime;

            if (payload.TryGetProperty("endTime", out var endProp) && TimeSpan.TryParse(endProp.GetString(), out var endTime))
                activity.EndTime = endTime;

            await _db.SaveActivityAsync(activity);
            _notificationService.ScheduleNotificationForActivity(activity);
        }
        else
        {
            throw new Exception($"Activity with ID {id} not found");
        }
    }

    private async Task HandleDeleteActivity(JsonElement payload)
    {
        var id = payload.GetProperty("id").GetInt32();
        var activity = await _db.GetActivityByIdAsync(id);
        if (activity != null)
        {
            await _db.DeleteActivityAsync(activity);
        }
        else
        {
            throw new Exception($"Activity with ID {id} not found");
        }
    }

    private async Task<string?> HandleFetchSchedule(JsonElement payload)
    {
        var startDateStr = payload.GetProperty("startDate").GetString();
        var endDateStr = payload.TryGetProperty("endDate", out var endDateProp) ? endDateProp.GetString() : startDateStr;

        if (DateTime.TryParse(startDateStr, out var startDate) && DateTime.TryParse(endDateStr, out var endDate))
        {
            var activities = await _db.GetActivitiesForDateRangeAsync(startDate, endDate);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Schedule from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}:");
            if (activities.Count == 0) sb.AppendLine("- Empty (no plans)");
            foreach (var a in activities)
            {
                sb.AppendLine($"- [Id: {a.Id}] {a.Date:yyyy-MM-dd} {a.StartTime:hh\\:mm} to {a.EndTime:hh\\:mm}: {a.Name} (Skippable: {a.IsFlexible})");
            }
            return sb.ToString();
        }
        
        throw new Exception("Invalid date format in fetch_schedule. Use YYYY-MM-DD.");
    }
}
