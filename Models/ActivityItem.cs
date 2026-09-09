using SQLite;
using System;

namespace RoutineKeeper.Models;

public class ActivityItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    // AI Scheduling properties
    public bool IsFlexible { get; set; } = true;
    public bool IsCompleted { get; set; } = false;

    [Ignore]
    public DayOfWeek DayOfWeek => Date.DayOfWeek;

    [Ignore]
    public TimeSpan Duration => EndTime - StartTime;
}
