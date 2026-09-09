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

    public ChatViewModel(AiAssistantService aiService, AgentActionExecutor actionExecutor, LocalDatabaseService db)
    {
        _aiService = aiService;
        _actionExecutor = actionExecutor;
        _db = db;

        _ = LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        var history = await _db.GetChatMessagesForDateAsync(DateTime.Today);
        if (history.Count == 0)
        {
            var welcome = new ChatMessage
            {
                Role = ChatRole.Assistant,
                Text = "Hey! What are your plans and goals for today?",
                Timestamp = DateTime.Now
            };
            await _db.SaveChatMessageAsync(welcome);
            Messages.Add(welcome);
            _aiService.LoadHistory(Messages);
        }
        else
        {
            foreach (var msg in history)
            {
                Messages.Add(msg);
            }
            _aiService.LoadHistory(Messages);
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsTyping) return;

        var userMessage = InputText.Trim();
        InputText = string.Empty;

        // Add user message to UI
        var userMsg = new ChatMessage
        {
            Role = ChatRole.User,
            Text = userMessage,
            Timestamp = DateTime.Now
        };
        await _db.SaveChatMessageAsync(userMsg);
        Messages.Add(userMsg);

        IsTyping = true;

        try
        {
            // Call AI
            var jsonResponse = await _aiService.SendMessageAsync(userMessage);

            // Execute background actions (db inserts)
            await _actionExecutor.ExecuteActionsAsync(jsonResponse);

            // Parse out the conversational reply to show the user
            string replyText = "I processed your request, but couldn't form a text response.";
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("response", out var responseProp))
                {
                    replyText = responseProp.GetString() ?? replyText;
                }
            }
            catch
            {
                replyText = "Error parsing AI response format.";
            }

            // Show AI response in UI
            var aiMsg = new ChatMessage
            {
                Role = ChatRole.Assistant,
                Text = replyText,
                Timestamp = DateTime.Now
            };
            await _db.SaveChatMessageAsync(aiMsg);
            Messages.Add(aiMsg);
        }
        catch (Exception ex)
        {
            var sysMsg = new ChatMessage
            {
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
