using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoutineKeeper.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace RoutineKeeper.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly GoogleAuthService _authService;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public LoginViewModel(GoogleAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var token = await _authService.LoginAsync();

            if (!string.IsNullOrEmpty(token))
            {
                // Persist the token across sessions
                Preferences.Set("AuthToken", token);

                // Navigate to the main app
                await Shell.Current.GoToAsync("///MainPage");
            }
            else
            {
                await ShowError("Sign-in failed. Please try again.");
            }
        }
        catch (Exception ex)
        {
            await ShowError($"Unexpected error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task ShowError(string message)
    {
        if (Application.Current?.MainPage != null)
            await Application.Current.MainPage.DisplayAlert("Sign-in Error", message, "OK");
    }
}
