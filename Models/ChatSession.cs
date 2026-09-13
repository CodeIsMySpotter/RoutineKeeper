using System;
using SQLite;

namespace RoutineKeeper.Models;

public class ChatSession
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
