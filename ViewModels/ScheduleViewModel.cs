using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoutineKeeper.Models;
using CommunityToolkit.Mvvm.Messaging;
using RoutineKeeper.Messages;
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

    [ObservableProperty]
    public partial System.DateTime CurrentMonthDate { get; set; } = System.DateTime.Today;

    [ObservableProperty]
    public partial bool IsAddTaskVisible { get; set; } = false;

    partial void OnSelectedDateChanged(System.DateTime value)
    {
        CurrentMonthDate = value;
        GenerateCalendar();
        LoadActivitiesAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void ToggleCalendar()
    {
        IsCalendarVisible = !IsCalendarVisible;
        if (IsCalendarVisible)
        {
            CurrentMonthDate = SelectedDate;
            GenerateCalendar();
        }
    }

    [RelayCommand]
    private void ToggleAddTask()
    {
        IsAddTaskVisible = !IsAddTaskVisible;
    }

    [RelayCommand]
    private void NextMonth()
    {
        CurrentMonthDate = CurrentMonthDate.AddMonths(1);
        GenerateCalendar();
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        CurrentMonthDate = CurrentMonthDate.AddMonths(-1);
        GenerateCalendar();
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
        var firstDayOfMonth = new DateTime(CurrentMonthDate.Year, CurrentMonthDate.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(CurrentMonthDate.Year, CurrentMonthDate.Month);
        
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
                IsCurrentMonth = currentDate.Month == CurrentMonthDate.Month,
                IsSelected = currentDate.Date == SelectedDate.Date,
                IsToday = currentDate.Date == DateTime.Today
            });
            currentDate = currentDate.AddDays(1);
        }
    }

    public ScheduleViewModel(LocalDatabaseService databaseService)
    {
        _databaseService = databaseService;
        WeakReferenceMessenger.Default.Register<ScheduleChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadActivitiesAsync();
            });
        });
    }

    [RelayCommand]
    private async Task LoadActivitiesAsync()
    {
        var items = await _databaseService.GetActivitiesForDateAsync(SelectedDate);
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
