using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using RoutineKeeper.Services;
using System.Windows.Input;
using RoutineKeeper.Models;

namespace RoutineKeeper.Views;

public partial class FloatingDock : ContentView
{
    public static readonly BindableProperty ActivePageProperty = BindableProperty.Create(
        nameof(ActivePage), typeof(string), typeof(FloatingDock), string.Empty, propertyChanged: OnActivePageChanged);

    public string ActivePage
    {
        get => (string)GetValue(ActivePageProperty);
        set => SetValue(ActivePageProperty, value);
    }

    private static void OnActivePageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FloatingDock dock && newValue is string page)
        {
            dock.ChatBorder.BackgroundColor = Colors.Transparent;
            dock.ScheduleBorder.BackgroundColor = Colors.Transparent;
            dock.DashboardBorder.BackgroundColor = Colors.Transparent;
            dock.NotesBorder.BackgroundColor = Colors.Transparent;
            dock.SettingsBorder.BackgroundColor = Colors.Transparent;

            var activeColor = Color.FromArgb("#33FFFFFF");

            if (page == "Chat") dock.ChatBorder.BackgroundColor = activeColor;
            else if (page == "Schedule") dock.ScheduleBorder.BackgroundColor = activeColor;
            else if (page == "Dashboard") dock.DashboardBorder.BackgroundColor = activeColor;
            else if (page == "Notes") dock.NotesBorder.BackgroundColor = activeColor;
        }
    }

    private bool _isThemePopupVisible;
    public bool IsThemePopupVisible
    {
        get => _isThemePopupVisible;
        set
        {
            _isThemePopupVisible = value;
            OnPropertyChanged();
        }
    }

    public ICommand GoToChatCommand { get; }
    public ICommand GoToScheduleCommand { get; }
    public ICommand GoToDashboardCommand { get; }
    public ICommand GoToNotesCommand { get; }
    
    public ICommand ToggleThemePopupCommand { get; }
    public ICommand SelectThemeCommand { get; }

    public FloatingDock()
    {
        InitializeComponent();

        GoToChatCommand = new Command(async () => await Shell.Current.GoToAsync("///ChatPage"));
        GoToScheduleCommand = new Command(async () => await Shell.Current.GoToAsync("///SchedulePage"));
        GoToDashboardCommand = new Command(async () => await Shell.Current.GoToAsync("///MainPage"));
        GoToNotesCommand = new Command(async () => await Shell.Current.GoToAsync("///NotesPage"));

        var themeService = IPlatformApplication.Current?.Services.GetService<ThemeService>();

        ToggleThemePopupCommand = new Command(() =>
        {
            IsThemePopupVisible = !IsThemePopupVisible;
        });

        SelectThemeCommand = new Command<string>((themeName) =>
        {
            if (themeService == null) return;
            
            if (themeName == "Default")
                themeService.SetTheme(ThemeType.Default);
            else if (themeName == "Dark")
                themeService.SetTheme(ThemeType.Dark);
            else if (themeName == "Catppuccin")
                themeService.SetTheme(ThemeType.Catppuccin);
                
            IsThemePopupVisible = false;
        });

        BindingContext = this;
    }
}
