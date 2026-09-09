using RoutineKeeper.ViewModels;

namespace RoutineKeeper;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is MainPageViewModel vm)
        {
            await vm.LoadDashboardDataAsync();
        }
    }
}
