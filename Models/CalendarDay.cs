using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RoutineKeeper.Models;

public partial class CalendarDay : ObservableObject
{
    [ObservableProperty]
    public partial DateTime Date { get; set; }

    [ObservableProperty]
    public partial bool IsCurrentMonth { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsToday { get; set; }
}
