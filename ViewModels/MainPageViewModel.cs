using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoutineKeeper.Services;

namespace RoutineKeeper.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly LocalDatabaseService _databaseService;
    private readonly ThemeService _themeService;

    [ObservableProperty]
    public partial int RestHours { get; set; } = 4;

    [ObservableProperty]
    public partial int RestMinutes { get; set; } = 30;

    [ObservableProperty]
    public partial string CurrentActivity { get; set; } = "Czas wolny";

    [ObservableProperty]
    public partial int TodayTotalTasks { get; set; } = 0;

    [ObservableProperty]
    public partial int TodayCompletedTasks { get; set; } = 0;

    public MainPageViewModel(LocalDatabaseService databaseService, ThemeService themeService)
    {
        _databaseService = databaseService;
        _themeService = themeService;
    }

    public async Task LoadDashboardDataAsync()
    {
        var todayActivities = await _databaseService.GetActivitiesForDateAsync(DateTime.Today);
        TodayTotalTasks = todayActivities.Count;
        TodayCompletedTasks = todayActivities.Count(a => a.IsCompleted);

        TimeSpan totalTaskTime = TimeSpan.Zero;
        foreach (var activity in todayActivities)
        {
            if (activity.EndTime > activity.StartTime)
            {
                totalTaskTime += (activity.EndTime - activity.StartTime);
            }
        }

        TimeSpan sleepTime = TimeSpan.FromHours(8); // Domyślnie 8 godzin snu
        TimeSpan totalDay = TimeSpan.FromHours(24);
        TimeSpan restTime = totalDay - sleepTime - totalTaskTime;

        if (restTime < TimeSpan.Zero)
            restTime = TimeSpan.Zero;

        RestHours = restTime.Hours;
        RestMinutes = restTime.Minutes;
    }
}


