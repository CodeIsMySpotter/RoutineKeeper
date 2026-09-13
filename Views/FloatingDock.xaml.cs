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
            var transparentColor = Colors.Transparent;
            dock.ChatBorder.BackgroundColor = transparentColor;
            dock.ScheduleBorder.BackgroundColor = transparentColor;
            dock.DashboardBorder.BackgroundColor = transparentColor;
            dock.NotesBorder.BackgroundColor = transparentColor;
            dock.SettingsBorder.BackgroundColor = transparentColor;

            dock.ChatBorder.RemoveDynamicResource(Border.BackgroundColorProperty);
            dock.ScheduleBorder.RemoveDynamicResource(Border.BackgroundColorProperty);
            dock.DashboardBorder.RemoveDynamicResource(Border.BackgroundColorProperty);
            dock.NotesBorder.RemoveDynamicResource(Border.BackgroundColorProperty);
            dock.SettingsBorder.RemoveDynamicResource(Border.BackgroundColorProperty);

            if (page == "Chat") dock.ChatBorder.SetDynamicResource(Border.BackgroundColorProperty, "PrimaryAccent");
            else if (page == "Schedule") dock.ScheduleBorder.SetDynamicResource(Border.BackgroundColorProperty, "PrimaryAccent");
            else if (page == "Dashboard") dock.DashboardBorder.SetDynamicResource(Border.BackgroundColorProperty, "PrimaryAccent");
            else if (page == "Notes") dock.NotesBorder.SetDynamicResource(Border.BackgroundColorProperty, "PrimaryAccent");
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

        GoToChatCommand = new Command(async () => await SafeNavigateAsync("//ChatPage"));
        GoToScheduleCommand = new Command(async () => await SafeNavigateAsync("//SchedulePage"));
        GoToDashboardCommand = new Command(async () => await SafeNavigateAsync("//MainPage"));
        GoToNotesCommand = new Command(async () => await SafeNavigateAsync("//NotesPage"));

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

    private async Task SafeNavigateAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }
    }
}
