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
            var token = await Task.Run(async () => await _authService.LoginAsync());

            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        Preferences.Set("AuthToken", token);
                        Application.Current.MainPage = new AppShell();
                    }
                    else
                    {
                        await ShowError("Sign-in failed. Please try again.");
                    }
                }
                catch (Exception navEx)
                {
                    await ShowError($"Navigation error: {navEx.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }
        catch (Exception ex)
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ShowError($"Unexpected error: {ex.Message}");
                IsBusy = false;
            });
        }
    }

    private static async Task ShowError(string message)
    {
        if (Application.Current?.MainPage != null)
            await Application.Current.Windows[0].Page.DisplayAlert("Sign-in Error", message, "OK");
    }
}
