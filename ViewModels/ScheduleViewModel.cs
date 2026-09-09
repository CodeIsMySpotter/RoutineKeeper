using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoutineKeeper.Models;
using RoutineKeeper.Services;
using System.Threading.Tasks;

namespace RoutineKeeper.ViewModels;

public partial class ScheduleViewModel : ObservableObject
{
    private readonly LocalDatabaseService _databaseService;

    [ObservableProperty]
    public partial ObservableCollection<ActivityItem> Activities { get; set; } = new();

    [ObservableProperty]
    public partial System.DateTime SelectedDate { get; set; } = System.DateTime.Today;

    [ObservableProperty]
    public partial bool IsCalendarVisible { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollection<CalendarDay> CalendarDays { get; set; } = new();

    partial void OnSelectedDateChanged(System.DateTime value)
    {
        GenerateCalendar();
        LoadActivitiesAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void ToggleCalendar()
    {
        IsCalendarVisible = !IsCalendarVisible;
        if (IsCalendarVisible)
        {
            GenerateCalendar();
        }
    }

    [RelayCommand]
    private void SelectDate(CalendarDay day)
    {
        if (day == null) return;
        SelectedDate = day.Date;
        IsCalendarVisible = false;
    }

    private void GenerateCalendar()
    {
        CalendarDays.Clear();
        var firstDayOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(SelectedDate.Year, SelectedDate.Month);
        
        // Oblicz od którego dnia tygodnia zaczynamy (Poniedziałek = 1)
        int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        if (startDayOfWeek == 0) startDayOfWeek = 7; // Niedziela jako 7 w systemie od poniedziałku

        var currentDate = firstDayOfMonth.AddDays(-(startDayOfWeek - 1));

        // 42 to 6 rzędów po 7 dni
        for (int i = 0; i < 42; i++)
        {
            CalendarDays.Add(new CalendarDay
            {
                Date = currentDate,
                IsCurrentMonth = currentDate.Month == SelectedDate.Month,
                IsSelected = currentDate.Date == SelectedDate.Date,
                IsToday = currentDate.Date == DateTime.Today
            });
            currentDate = currentDate.AddDays(1);
        }
    }

    public ScheduleViewModel(LocalDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    private async Task LoadActivitiesAsync()
    {
        var items = await _databaseService.GetActivitiesAsync();
        Activities.Clear();
        foreach (var item in items)
        {
            Activities.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddMockActivityAsync()
    {
        var activity = new ActivityItem
        {
            Name = "Siłownia",
            Date = System.DateTime.Today,
            StartTime = new System.TimeSpan(17, 0, 0),
            EndTime = new System.TimeSpan(19, 0, 0)
        };
        await _databaseService.SaveActivityAsync(activity);
        await LoadActivitiesAsync();
    }
}
