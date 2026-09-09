# RoutineKeeper

RoutineKeeper is a cross-platform desktop and mobile application built with **.NET MAUI**. It is designed to help users manage their daily routines, track habits, and plan their day with the help of an intelligent AI assistant. 

The application focuses on reducing stress through a calming, minimalist, and dark-themed UI (GitHub Dark inspired).

## ✨ Features

*   **Intelligent AI Assistant:** Chat with an integrated Google Gemini AI that can automatically read your schedule and seamlessly add new activities via a background JSON execution engine.
*   **Daily Planner:** Manage time blocks for your day (e.g., Work, Gym, Study, Rest) with a beautiful timeline view.
*   **Dashboard & Rest Calculator:** Automatically calculates your remaining free time based on your scheduled tasks and expected sleep time, promoting a healthy work-life balance.
*   **Notes System:** A built-in scratchpad for quick thoughts and ideas.
*   **Google OAuth Integration:** Secure, native login flow using Google authentication.
*   **100% Local Storage:** Your data stays yours. Everything is stored locally on your device using an embedded SQLite database.

---

## 🚀 Getting Started

### Prerequisites

To build and run RoutineKeeper, you need:
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   The `.NET MAUI` workload installed.
*   A Google Cloud project with OAuth credentials (Client ID) configured for desktop/native apps.
*   A [Google Gemini API Key](https://aistudio.google.com) for the AI chat features.

### Configuration

For security, API keys and Client IDs should not be hardcoded. The application is configured to read from `appsettings.json` embedded in the app, but **it is highly recommended to use Environment Variables** for local development to keep your repository clean.

#### Method 1: Environment Variables (Recommended)
Set the following environment variables on your system:

**Windows (Command Prompt):**
```cmd
setx GoogleAuth__ClientId "YOUR_GOOGLE_CLIENT_ID"
setx GeminiApi__ApiKey "YOUR_GEMINI_API_KEY"
```

#### Method 2: appsettings.json
Alternatively, you can edit the `appsettings.json` file in the root of the project:
```json
{
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "RedirectUri": "routinekeeper://auth"
  },
  "GeminiApi": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```

### Build and Run

To run the application locally on Windows:

1. Clone the repository.
2. Open a terminal in the project directory.
3. Run the following command:
```bash
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

*Alternatively, open `RoutineKeeper.csproj` in Visual Studio 2022+ and hit F5.*

---

## 🏗️ Architecture & Technology Stack

*   **Framework:** .NET MAUI (Multi-platform App UI) targeting .NET 10.
*   **Pattern:** MVVM (Model-View-ViewModel) using the `CommunityToolkit.Mvvm` package.
*   **Database:** `sqlite-net-pcl` for local, offline-first data storage.
*   **AI Integration:** Raw HTTP requests to Google's Generative AI API (Gemini 1.5 Flash) with strict JSON schema enforcing for background agent execution.
*   **Dependency Injection:** Built-in Microsoft DI container used heavily in `MauiProgram.cs` for Services and ViewModels.

### Project Structure
*   `Models/` - Data representations (`ActivityItem`, `NoteItem`, `ChatMessage`).
*   `ViewModels/` - UI Logic bound to the pages (`MainPageViewModel`, `ChatViewModel`, etc.).
*   `Views/` - XAML UI definitions.
*   `Services/` - Core business logic:
    *   `LocalDatabaseService.cs`: SQLite CRUD operations.
    *   `GoogleAuthService.cs`: OAuth PKCE flow.
    *   `AiAssistantService.cs`: Gemini API communication.
    *   `AgentActionExecutor.cs`: Parses JSON actions from the AI and executes them on the DB.
*   `Converters/` - Value converters for XAML bindings.

---

## 🎨 Design System

RoutineKeeper strictly follows a specialized design system to maximize user comfort:
*   **Theme:** Deep Dark Mode.
*   **Colors:** 
    *   Backgrounds: `#0D1117`, `#161B22`
    *   Borders: `#30363D`
    *   Accent: Soothing Emerald Green. (No aggressive neon colors).
*   **Typography:** Clean sans-serif fonts with large, readable counters and timers.
