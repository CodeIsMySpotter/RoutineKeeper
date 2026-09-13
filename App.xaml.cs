using Microsoft.Extensions.DependencyInjection;

namespace RoutineKeeper;

public partial class App : Application
{
	public App(RoutineKeeper.Services.ThemeService themeService, IServiceProvider serviceProvider)
	{
		InitializeComponent();
		
		// Inicjalizację motywu robimy bezpiecznie na głównym wątku,
		// kiedy zasoby i okno są już załadowane
		MainThread.BeginInvokeOnMainThread(() =>
		{
			themeService.InitializeTheme();
		});

        if (Preferences.ContainsKey("AuthToken"))
        {
            MainPage = new AppShell();
        }
        else
        {
            MainPage = serviceProvider.GetRequiredService<RoutineKeeper.Views.LoginPage>();
        }
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(MainPage);
		
		var displayInfo = DeviceDisplay.MainDisplayInfo;
		var screenHeight = displayInfo.Height / displayInfo.Density;

		double newWidth = 400;
		double newHeight = 850;

		// Zabezpieczenie dla mniejszych ekranów (np. laptopów)
		if (screenHeight > 0 && screenHeight < 950)
		{
			newHeight = screenHeight - 100;
		}

		window.Width = newWidth;
		window.Height = newHeight;
		window.MinimumWidth = newWidth;
		window.MinimumHeight = newHeight;
		window.MaximumWidth = newWidth;
		window.MaximumHeight = newHeight;

		return window;
	}
}
