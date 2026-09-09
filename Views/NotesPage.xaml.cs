using Microsoft.Maui.Controls;
using RoutineKeeper.Models;
using RoutineKeeper.ViewModels;

namespace RoutineKeeper.Views;

public partial class NotesPage : ContentPage
{
    public NotesPage(NotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is NotesViewModel vm)
        {
            await vm.LoadNotesAsync();
        }
    }
}

public class NoteTemplateSelector : DataTemplateSelector
{
    public DataTemplate AddNoteTemplate { get; set; } = default!;
    public DataTemplate NormalNoteTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is NoteItem note)
        {
            if (note.Id == -1)
                return AddNoteTemplate;
        }
        return NormalNoteTemplate;
    }
}
