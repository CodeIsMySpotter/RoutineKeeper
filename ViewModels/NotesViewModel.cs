using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoutineKeeper.Models;
using RoutineKeeper.Services;

namespace RoutineKeeper.ViewModels;

public partial class NotesViewModel : ObservableObject
{
    private readonly LocalDatabaseService _databaseService;

    [ObservableProperty]
    public partial ObservableCollection<NoteItem> Notes { get; set; } = new();

    public NotesViewModel(LocalDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task LoadNotesAsync()
    {
        var dbNotes = await _databaseService.GetNotesAsync();
        
        Notes.Clear();
        // Zawsze pierwszy kafelek to przycisk dodawania nowej notatki (Id = -1)
        Notes.Add(new NoteItem { Id = -1, Title = "Nowa notatka" });
        
        foreach (var note in dbNotes)
        {
            Notes.Add(note);
        }
    }

    [RelayCommand]
    private async Task NoteTappedAsync(NoteItem note)
    {
        if (note == null) return;

        if (note.Id == -1)
        {
            // Dodawanie nowej notatki (TODO: Otwórz modal / nową stronę)
            var newNote = new NoteItem { Title = "Nowa Notatka", Content = "Treść..." };
            await _databaseService.SaveNoteAsync(newNote);
            await LoadNotesAsync();
        }
        else
        {
            // Edycja / Podgląd (TODO)
        }
    }
}
