using System;
using SQLite;

namespace RoutineKeeper.Models;

public enum ChatRole
{
    User,
    Assistant,
    System
}

public class ChatMessage
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string SessionId { get; set; } = string.Empty;

    [Indexed]
    public DateTime Date { get; set; } = DateTime.Today;

    public ChatRole Role { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Ignore]
    public bool IsUser => Role == ChatRole.User;
    
    [Ignore]
    public bool IsAssistant => Role == ChatRole.Assistant;
}
