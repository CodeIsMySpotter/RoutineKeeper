using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Authentication;

namespace RoutineKeeper.Services;

public class GoogleAuthService
{
    private readonly string _clientId;
    private readonly string _windowsClientId;
    private readonly string _windowsClientSecret;
    private readonly string _redirectUri;

    public GoogleAuthService(IConfiguration configuration)
    {
        // Reads from appsettings.json → GoogleAuth section
        _clientId = configuration["GoogleAuth:ClientId"] ?? string.Empty;
        _windowsClientId = configuration["GoogleAuth:WindowsClientId"] ?? string.Empty;
        _windowsClientSecret = configuration["GoogleAuth:WindowsClientSecret"] ?? string.Empty;
        _redirectUri = configuration["GoogleAuth:RedirectUri"] ?? "routinekeeper://auth";
    }

    public bool IsConfigured => (!string.IsNullOrWhiteSpace(_clientId) && _clientId != "YOUR_GOOGLE_CLIENT_ID_HERE") || 
                                (!string.IsNullOrWhiteSpace(_windowsClientId) && _windowsClientId != "YOUR_DESKTOP_CLIENT_ID");

    public async Task<string?> LoginAsync()
    {
        if (!IsConfigured)
        {
            // Graceful mock while Client ID is not yet set
            await Task.Delay(1500);
            return "MOCK_GOOGLE_TOKEN_123";
        }

        try
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

#if WINDOWS
            if (string.IsNullOrWhiteSpace(_windowsClientId) || string.IsNullOrWhiteSpace(_windowsClientSecret))
                throw new Exception("Brak 'WindowsClientId' lub 'WindowsClientSecret' w appsettings.json (Desktop Client ID wymaga obu).");

            var winRedirectUri = "http://127.0.0.1:5000/";
            var winAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                          $"?client_id={_windowsClientId}" +
                          $"&response_type=code" +
                          $"&redirect_uri={Uri.EscapeDataString(winRedirectUri)}" +
                          $"&scope=openid%20email%20profile" +
                          $"&code_challenge={codeChallenge}" +
                          $"&code_challenge_method=S256";

            using var listener = new System.Net.HttpListener();
            listener.Prefixes.Add(winRedirectUri);
            listener.Start();

            await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(winAuthUrl).ConfigureAwait(false);

            System.Net.HttpListenerContext context;
            while (true)
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
                if (context.Request.Url?.AbsolutePath.Contains("favicon.ico") == true)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                    continue;
                }
                break;
            }

            var req = context.Request;
            var res = context.Response;

            string? code = req.QueryString["code"];
            
            var responseString = "<html><body>Zalogowano pomyślnie. Mozesz zamknac te karte i wrocic do aplikacji.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            res.ContentLength64 = buffer.Length;
            var output = res.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            listener.Stop();

            if (!string.IsNullOrEmpty(code))
            {
                return await ExchangeCodeForTokenAsync(code, codeVerifier, _windowsClientId, winRedirectUri, _windowsClientSecret).ConfigureAwait(false);
            }
#else
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                          $"?client_id={_clientId}" +
                          $"&response_type=code" +
                          $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                          $"&scope=openid%20email%20profile" +
                          $"&code_challenge={codeChallenge}" +
                          $"&code_challenge_method=S256";

            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri(_redirectUri)).ConfigureAwait(false);

            if (authResult?.Properties.TryGetValue("code", out var code) == true)
            {
                return await ExchangeCodeForTokenAsync(code, codeVerifier, _clientId, _redirectUri, null).ConfigureAwait(false);
            }
#endif

            throw new Exception("Przerwano logowanie lub brak kodu zwrotnego.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd logowania: {ex.Message}");
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(string code, string codeVerifier, string clientId, string redirectUri, string? clientSecret)
    {
        using var client = new HttpClient();
        
        var requestData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("code_verifier", codeVerifier),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            requestData.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
        }

        var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(requestData)).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
            {
                return tokenElement.GetString();
            }
            throw new Exception("Brak access_token w odpowiedzi od Google.");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"Token exchange failed: {response.StatusCode} - {error}");
        }
    }

    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
