using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace RoutineKeeper.Platforms.Windows;

public class CustomEntryHandler : EntryHandler
{
    protected override void ConnectHandler(TextBox platformView)
    {
        base.ConnectHandler(platformView);

        // Usuń natywne obramowanie i tło WinUI TextBox
        platformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        platformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
        platformView.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);

        // Usuń ramki w stanach focused i pointerover poprzez Style
        platformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
        platformView.Resources["TextControlBorderBrush"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBorderBrushPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBorderBrushFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBorderBrushDisabled"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBackground"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBackgroundPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBackgroundFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        platformView.Resources["TextControlBackgroundDisabled"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }
}
