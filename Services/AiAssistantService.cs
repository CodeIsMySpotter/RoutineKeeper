using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RoutineKeeper.Models;

namespace RoutineKeeper.Services;

public class AiAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly LocalDatabaseService _db;
    private readonly AgentActionExecutor _actionExecutor;
    private readonly string _apiKey;
    
    public class HistoryItem
    {
        public string role { get; set; }
        public object parts { get; set; }
    }
    
    private readonly List<HistoryItem> _history = new();

    public AiAssistantService(IConfiguration config, LocalDatabaseService db, AgentActionExecutor actionExecutor)
    {
        _httpClient = new HttpClient();
        _db = db;
        _actionExecutor = actionExecutor;
        _apiKey = config["GeminiApi:ApiKey"] ?? string.Empty;
    }

    public void LoadHistory(IEnumerable<ChatMessage> messages)
    {
        _history.Clear();
        foreach (var msg in messages)
        {
            if (msg.Role == ChatRole.User)
            {
                _history.Add(new HistoryItem { role = "user", parts = new[] { new { text = msg.Text } } });
            }
            else if (msg.Role == ChatRole.Assistant)
            {
                _history.Add(new HistoryItem { role = "model", parts = new[] { new { text = msg.Text } } });
            }
        }
    }

    private void TrimHistory()
    {
        const int MaxHistoryItems = 10;
        while (_history.Count > MaxHistoryItems)
        {
            _history.RemoveAt(0);
        }
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "YOUR_GEMINI_API_KEY_HERE";

    private async Task<string> AskGeminiAPIAsync(string systemPrompt, string userMessage)
    {
        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userMessage } } }
            },
            generationConfig = new
            {
                temperature = 0.2
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={_apiKey}";

        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API Error: {err}");
        }

        var responseStr = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseStr);
        var root = document.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                candidateContent.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var text))
            {
                var txt = text.GetString()?.Trim();
                // Clean markdown JSON if present
                if (txt != null)
                {
                    if (txt.StartsWith("```json")) txt = txt.Substring(7);
                    else if (txt.StartsWith("```")) txt = txt.Substring(3);
                    if (txt.EndsWith("```")) txt = txt.Substring(0, txt.Length - 3);
                    return txt.Trim();
                }
            }
        }

        throw new Exception("Nie udało się sparsować odpowiedzi od Gemini.");
    }

    private async Task<string> AskRouterAgentAsync(string userMessage)
    {
        string systemPrompt = @"You are a Router Agent for a calendar app. 
Your ONLY job is to classify the user's intent into exactly ONE of the following tags:
ADD_ACTIVITY
ADD_RECURRING_ACTIVITY
DELETE_ACTIVITY
UPDATE_ACTIVITY
FETCH_SCHEDULE
CLEAR_SCHEDULE
GENERAL_CHAT

Rules:
- If the user wants to add a one-time event, return ADD_ACTIVITY.
- If the user wants to add an event that repeats on certain days (e.g. classes, gym every tuesday), return ADD_RECURRING_ACTIVITY.
- If the user wants to clear or delete ALL activities/events from the schedule, return CLEAR_SCHEDULE.
- If the user is just saying hello or asking a general question, return GENERAL_CHAT.
- RETURN ONLY THE TAG. NO OTHER TEXT.";

        string response = await AskGeminiAPIAsync(systemPrompt, userMessage);
        
        if (response.Contains("ADD_RECURRING")) return "ADD_RECURRING_ACTIVITY";
        if (response.Contains("ADD_ACTIVITY")) return "ADD_ACTIVITY";
        if (response.Contains("DELETE")) return "DELETE_ACTIVITY";
        if (response.Contains("UPDATE")) return "UPDATE_ACTIVITY";
        if (response.Contains("FETCH")) return "FETCH_SCHEDULE";
        if (response.Contains("CLEAR")) return "CLEAR_SCHEDULE";
        
        return "GENERAL_CHAT";
    }

    private async Task<string> AskDataExtractorAgentAsync(string userMessage, string intent)
    {
        var today = DateTime.Today;
        string scheduleContext = "";
        
        if (intent == "DELETE_ACTIVITY" || intent == "UPDATE_ACTIVITY")
        {
            var upcoming = await _db.GetActivitiesForDateRangeAsync(today.AddDays(-2), today.AddDays(7));
            var sb = new StringBuilder();
            sb.AppendLine("Current schedule context (to help you find the correct ID):");
            if (upcoming.Count == 0) sb.AppendLine("No activities found.");
            foreach (var a in upcoming)
            {
                sb.AppendLine($"- [Id: {a.Id}] {a.Date:yyyy-MM-dd} {a.Name} ({a.StartTime:hh\\:mm}-{a.EndTime:hh\\:mm})");
            }
            scheduleContext = sb.ToString();
        }

        string systemPrompt = $@"You are a JSON Data Extractor for a calendar app.
Today's date is {today:yyyy-MM-dd}.
The user wants to perform: {intent}.
Extract the details from the user's message and return ONLY a valid JSON object.

CRITICAL RULES:
1. ALL DATES must be EXACTLY in YYYY-MM-DD format. You must calculate the date yourself (e.g., if today is 2026-09-13 and user says 'end of october', you write '2026-10-31').
2. Do not write text in date fields.

{scheduleContext}

Formats:
ADD_ACTIVITY: {{ ""title"": ""Name"", ""date"": ""YYYY-MM-DD"", ""startTime"": ""HH:MM"", ""endTime"": ""HH:MM"" }}
ADD_RECURRING_ACTIVITY: {{ ""title"": ""Name"", ""startTime"": ""HH:MM"", ""endTime"": ""HH:MM"", ""startDate"": ""YYYY-MM-DD"", ""endDate"": ""YYYY-MM-DD"", ""daysOfWeek"": [0, 3] }} (0=Niedziela, 1=Poniedziałek, 2=Wtorek, 3=Środa, 4=Czwartek, 5=Piątek, 6=Sobota)
DELETE_ACTIVITY: {{ ""id"": 123 }}
UPDATE_ACTIVITY: {{ ""id"": 123, ""title"": ""New Name"", ""startTime"": ""HH:MM"" }} (Include only fields that change)
CLEAR_SCHEDULE: {{ }} (Empty JSON object)

Return ONLY the JSON. Do not include any explanations.";

        return await AskGeminiAPIAsync(systemPrompt, userMessage);
    }

    private async Task<string> AskResponderAgentAsync(string userMessage, string executionResult)
    {
        string systemPrompt = @"You are a friendly AI assistant in a calendar app.
The user asked a request, and the system executed it.
System execution result: " + executionResult + @"

Write a short, friendly conversational response to the user in Polish confirming what was done. Keep it very short (1-2 sentences).";

        return await AskGeminiAPIAsync(systemPrompt, $"User request: {userMessage}\nSystem result: {executionResult}");
    }

    private async Task<string> AskGeneralChatAgentAsync(string userMessage)
    {
        string systemPrompt = "You are a helpful calendar assistant. The user is asking a general question. Keep your answers brief and in Polish.";
        
        var contentsList = new List<object>();
        foreach (var item in _history)
        {
            contentsList.Add(new { role = item.role, parts = item.parts });
        }

        // Add the current user message to contentsList
        contentsList.Add(new { role = "user", parts = new[] { new { text = userMessage } } });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = contentsList,
            generationConfig = new
            {
                temperature = 0.7
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={_apiKey}";

        var response = await _httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            return "Wystąpił problem z połączeniem z API Gemini.";
        }

        var responseStr = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseStr);
        var root = document.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                candidateContent.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var text))
            {
                return text.GetString()?.Trim();
            }
        }
        return "Nie udało się sparsować odpowiedzi od Gemini.";
    }

    public async Task<string> SendMessageAsync(string userMessage, byte[] imageBytes = null, string imageMimeType = null, Action<string> reportProgress = null)
    {
        if (!IsConfigured) return "Skonfiguruj klucz API w appsettings.json, aby korzystać z asystenta.";

        try
        {
            reportProgress?.Invoke("Analizuję intencję...");
            string intent = await AskRouterAgentAsync(userMessage);
            System.Diagnostics.Debug.WriteLine($"[ROUTER] Intent: {intent}");

            if (intent == "GENERAL_CHAT")
            {
                reportProgress?.Invoke("Piszę odpowiedź...");
                string response = await AskGeneralChatAgentAsync(userMessage);
                
                // Save to history after successful interaction
                _history.Add(new HistoryItem { role = "user", parts = new[] { new { text = userMessage } } });
                _history.Add(new HistoryItem { role = "model", parts = new[] { new { text = response } } });
                TrimHistory();
                
                return response;
            }
            else
            {
                reportProgress?.Invoke("Wyciągam dane...");
                string jsonPayload = await AskDataExtractorAgentAsync(userMessage, intent);
                System.Diagnostics.Debug.WriteLine($"[EXTRACTOR] Payload: {jsonPayload}");

                reportProgress?.Invoke("Zapisuję w kalendarzu...");
                string executionResult = await _actionExecutor.ExecuteSpecificActionAsync(intent, jsonPayload);

                reportProgress?.Invoke("Generuję podsumowanie...");
                string finalResponse = await AskResponderAgentAsync(userMessage, executionResult);
                
                // Save to history
                _history.Add(new HistoryItem { role = "user", parts = new[] { new { text = userMessage } } });
                _history.Add(new HistoryItem { role = "model", parts = new[] { new { text = finalResponse } } });
                TrimHistory();
                
                return finalResponse;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AiAssistantService] Error: {ex.Message}");
            return $"Wystąpił błąd podczas analizy: {ex.Message}";
        }
    }

    public void ClearHistory()
    {
        _history.Clear();
    }
}
