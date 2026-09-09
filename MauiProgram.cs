using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoutineKeeper.Services;
using RoutineKeeper.ViewModels;
using RoutineKeeper.Views;
using System.Reflection;

namespace RoutineKeeper;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureMauiHandlers(handlers =>
			{
#if WINDOWS
				handlers.AddHandler<Microsoft.Maui.Controls.Entry, RoutineKeeper.Platforms.Windows.CustomEntryHandler>();
#endif
			});

#if WINDOWS
		// Usuń obramowanie i tło natywnego WinUI TimePicker
		Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
		{
			var tp = handler.PlatformView;
			if (tp == null) return;
			tp.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
			tp.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			tp.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
			tp.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
			tp.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
			tp.Padding = new Microsoft.UI.Xaml.Thickness(0);
			tp.Margin = new Microsoft.UI.Xaml.Thickness(0);
			tp.Resources["TimePickerBorderBrush"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.Resources["TimePickerBorderBrushPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.Resources["TimePickerBorderBrushPressed"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.Resources["TimePickerBackground"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.Resources["TimePickerBackgroundPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.Resources["TimePickerBackgroundPressed"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
			tp.Resources["TimePickerMinuteIncrement"] = 1;
		});
#endif

		// Load appsettings.json embedded in the binary
		var configBuilder = new ConfigurationBuilder();
		
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("RoutineKeeper.appsettings.json");
		if (stream != null)
		{
			configBuilder.AddJsonStream(stream);
		}

		// Allow environment variables to override appsettings
		configBuilder.AddEnvironmentVariables();

		var config = configBuilder.Build();
		builder.Configuration.AddConfiguration(config);

		// Register services
		builder.Services.AddSingleton<LocalDatabaseService>();
		builder.Services.AddSingleton<GoogleAuthService>();
		builder.Services.AddSingleton<ThemeService>();
		builder.Services.AddSingleton<AiAssistantService>();
		builder.Services.AddSingleton<AgentActionExecutor>();
		builder.Services.AddSingleton<NotificationService>();

		// Register ViewModels and Views
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddSingleton<MainPageViewModel>();
		builder.Services.AddSingleton<MainPage>();
        
		builder.Services.AddTransient<ScheduleViewModel>();
		builder.Services.AddTransient<SchedulePage>();

		builder.Services.AddTransient<NotesViewModel>();
		builder.Services.AddTransient<NotesPage>();

		builder.Services.AddTransient<ChatViewModel>();
		builder.Services.AddTransient<ChatPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
