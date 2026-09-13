using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoutineKeeper.Models;
using RoutineKeeper.Services;

namespace RoutineKeeper.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly AiAssistantService _aiService;
    private readonly AgentActionExecutor _actionExecutor;
    private readonly LocalDatabaseService _db;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsTyping { get; set; } = false;

    [ObservableProperty]
    public partial string TypingStatus { get; set; } = string.Empty;

    [ObservableProperty]
    public partial byte[] SelectedImageBytes { get; set; }

    [ObservableProperty]
    public partial string SelectedImageMimeType { get; set; }

    [ObservableProperty]
    public partial bool IsImageAttached { get; set; }

    [ObservableProperty]
    public partial bool IsHistoryVisible { get; set; }

    [ObservableProperty]
    public partial string CurrentSessionId { get; set; }

    public ObservableCollection<ChatSession> Sessions { get; } = new();

    public ChatViewModel(AiAssistantService aiService, AgentActionExecutor actionExecutor, LocalDatabaseService db)
    {
        _aiService = aiService;
        _actionExecutor = actionExecutor;
        _db = db;

        _ = InitializeSessionAsync();
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var customFileType = new Microsoft.Maui.Storage.FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.image", "com.adobe.pdf", "public.calendar-event", "public.text" } },
                { DevicePlatform.Android, new[] { "image/*", "application/pdf", "text/calendar", "text/plain" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".pdf", ".ics", ".txt" } },
                { DevicePlatform.MacCatalyst, new[] { "public.image", "com.adobe.pdf", "public.calendar-event", "public.text" } },
            });

            var result = await Microsoft.Maui.Storage.FilePicker.Default.PickAsync(new Microsoft.Maui.Storage.PickOptions
            {
                PickerTitle = "Wybierz plik (obraz, PDF, ICS)",
                FileTypes = customFileType
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new System.IO.MemoryStream();
                await stream.CopyToAsync(ms);
                SelectedImageBytes = ms.ToArray();
                SelectedImageMimeType = result.FileName.EndsWith(".ics", StringComparison.OrdinalIgnoreCase) ? "text/calendar" : result.ContentType;
                IsImageAttached = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"File picking failed: {ex.Message}");
        }
    }

    private async Task InitializeSessionAsync()
    {
        var sessions = await _db.GetChatSessionsAsync();
        Sessions.Clear();
        foreach (var s in sessions) Sessions.Add(s);

        await CreateNewSessionAsync();
    }

    [RelayCommand]
    private async Task CreateNewSessionAsync()
    {
        IsHistoryVisible = false;
        Messages.Clear();
        
        var newSession = new ChatSession { Title = $"Sesja z {DateTime.Now:dd.MM HH:mm}" };
        await _db.SaveChatSessionAsync(newSession);
        
        CurrentSessionId = newSession.Id;
        Sessions.Insert(0, newSession);

        var welcome = new ChatMessage
        {
            SessionId = CurrentSessionId,
            Role = ChatRole.Assistant,
            Text = "Hej! Jakie masz plany na dzisiaj?",
            Timestamp = DateTime.Now
        };
        await _db.SaveChatMessageAsync(welcome);
        Messages.Add(welcome);
        _aiService.LoadHistory(Messages);
    }

    [RelayCommand]
    private async Task SelectSessionAsync(ChatSession session)
    {
        if (session == null) return;
        
        IsHistoryVisible = false;
        CurrentSessionId = session.Id;
        Messages.Clear();

        var history = await _db.GetChatMessagesForSessionAsync(session.Id);
        foreach (var msg in history)
        {
            Messages.Add(msg);
        }
        _aiService.LoadHistory(Messages);
    }

    [RelayCommand]
    private void ToggleHistory()
    {
        IsHistoryVisible = !IsHistoryVisible;
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if ((string.IsNullOrWhiteSpace(InputText) && !IsImageAttached) || IsTyping) return;

        var userMessage = string.IsNullOrWhiteSpace(InputText) ? "[Wysłano obraz]" : InputText.Trim();
        InputText = string.Empty;

        // Save image state locally for the first API call
        byte[] imageBytes = SelectedImageBytes;
        string imageMimeType = SelectedImageMimeType;
        
        // Clear UI state immediately
        SelectedImageBytes = null;
        SelectedImageMimeType = null;
        IsImageAttached = false;

        // Add user message to UI
        var userMsg = new ChatMessage
        {
            SessionId = CurrentSessionId,
            Role = ChatRole.User,
            Text = userMessage,
            Timestamp = DateTime.Now
        };
        await _db.SaveChatMessageAsync(userMsg);
        Messages.Add(userMsg);

        IsTyping = true;

        try
        {
            TypingStatus = "Analizuję...";
            
            var progress = new Action<string>(status => 
            {
                MainThread.BeginInvokeOnMainThread(() => TypingStatus = status);
            });

            string finalReplyText = await _aiService.SendMessageAsync(
                userMessage, 
                imageBytes, 
                imageMimeType,
                progress);

            // Show final AI response in UI only if it has text to say
            if (!string.IsNullOrWhiteSpace(finalReplyText))
            {
                var aiMsg = new ChatMessage
                {
                    SessionId = CurrentSessionId,
                    Role = ChatRole.Assistant,
                    Text = finalReplyText,
                    Timestamp = DateTime.Now
                };
                await _db.SaveChatMessageAsync(aiMsg);
                Messages.Add(aiMsg);
            }
        }
        catch (Exception ex)
        {
            var sysMsg = new ChatMessage
            {
                SessionId = CurrentSessionId,
                Role = ChatRole.System,
                Text = $"System Error: {ex.Message}",
                Timestamp = DateTime.Now
            };
            await _db.SaveChatMessageAsync(sysMsg);
            Messages.Add(sysMsg);
        }
        finally
        {
            IsTyping = false;
        }
    }
}
