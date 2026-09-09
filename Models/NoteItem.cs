using SQLite;
using System;

namespace RoutineKeeper.Models;

public class NoteItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Termin wygaśnięcia (null oznacza notatkę bezterminową, np. przepis)
    public DateTime? ExpirationDate { get; set; }
}
