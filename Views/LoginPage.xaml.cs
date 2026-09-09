using Microsoft.Maui.Controls;
using RoutineKeeper.ViewModels;

namespace RoutineKeeper.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
