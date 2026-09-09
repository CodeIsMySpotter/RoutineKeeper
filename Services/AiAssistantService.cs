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
    private readonly string _apiKey;
    
    // Maintain simple history
    private readonly List<object> _history = new();

    public AiAssistantService(IConfiguration config, LocalDatabaseService db)
    {
        _httpClient = new HttpClient();
        _db = db;
        _apiKey = config["GeminiApi:ApiKey"] ?? string.Empty;
    }

    public void LoadHistory(IEnumerable<ChatMessage> messages)
    {
        _history.Clear();
        foreach (var msg in messages)
        {
            if (msg.Role == ChatRole.User)
            {
                _history.Add(new { role = "user", parts = new[] { new { text = msg.Text } } });
            }
            else if (msg.Role == ChatRole.Assistant)
            {
                // We must format the assistant's past replies as valid JSON 
                // so the model sees itself strictly following the JSON rule.
                var fakeJsonResponse = $"{{\n  \"response\": \"{msg.Text}\",\n  \"actions\": []\n}}";
                _history.Add(new { role = "model", parts = new[] { new { text = fakeJsonResponse } } });
            }
        }
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "YOUR_GEMINI_API_KEY_HERE";

    public async Task<string> SendMessageAsync(string userMessage)
    {
        if (!IsConfigured)
            return "{\"response\": \"Please configure the Gemini API key in appsettings.json to use the assistant.\", \"actions\": []}";

        // Prepare context
        var today = DateTime.Today;
        var activities = await _db.GetActivitiesForDateAsync(today);
        var scheduleContext = "Current schedule for today:\n";
        if (activities.Count == 0) scheduleContext += "- Empty (no plans)\n";
        foreach (var a in activities)
            scheduleContext += $"- {a.StartTime:hh\\:mm} to {a.EndTime:hh\\:mm}: {a.Name}\n";

        var systemPrompt = $@"You are the RoutineKeeper AI Assistant. You help the user plan their day.
Today's date is {today:yyyy-MM-dd}.
{scheduleContext}

You MUST ALWAYS respond in valid JSON format ONLY. Do not use Markdown block formatting (like ```json).
Your response must strictly follow this JSON schema:
{{
  ""response"": ""Your conversational reply to the user"",
  ""actions"": [
    {{
      ""type"": ""add_activity"",
      ""payload"": {{
        ""title"": ""Activity Name"",
        ""date"": ""YYYY-MM-DD"",
        ""startTime"": ""HH:MM"",
        ""endTime"": ""HH:MM""
      }}
    }}
  ]
}}
If no actions are needed, return an empty array for actions: []";

        _history.Add(new { role = "user", parts = new[] { new { text = userMessage } } });

        var requestBody = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = _history,
            generationConfig = new { response_mime_type = "application/json" }
        };

        var jsonPayload = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Gemini API Error: {responseString}");
                return $"{{\"response\": \"API Error: {response.StatusCode}\", \"actions\": []}}";
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            
            // Extract the text from the Gemini response structure
            var generatedText = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (generatedText != null)
            {
                _history.Add(new { role = "model", parts = new[] { new { text = generatedText } } });
                return generatedText;
            }

            return "{\"response\": \"I could not generate a response.\", \"actions\": []}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI request failed: {ex.Message}");
            return $"{{\"response\": \"Network error or parsing failure: {ex.Message}\", \"actions\": []}}";
        }
    }

    public void ClearHistory()
    {
        _history.Clear();
    }
}
