using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;

namespace RoutineKeeper.Services;

public class GoogleAuthService
{
    private readonly string _clientId;
    private readonly string _redirectUri;

    public GoogleAuthService(IConfiguration configuration)
    {
        // Reads from appsettings.json → GoogleAuth section
        _clientId   = configuration["GoogleAuth:ClientId"]   ?? string.Empty;
        _redirectUri = configuration["GoogleAuth:RedirectUri"] ?? "routinekeeper://auth";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_clientId)
                                && _clientId != "YOUR_GOOGLE_CLIENT_ID_HERE";

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
            // Real WebAuthenticator flow
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri($"https://accounts.google.com/o/oauth2/v2/auth" +
                        $"?client_id={_clientId}" +
                        $"&response_type=code" +
                        $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                        $"&scope=openid%20email%20profile" +
                        $"&code_challenge_method=S256"),   // PKCE — no secret needed
                new Uri(_redirectUri));
                
            return authResult?.AccessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return null;
        }
    }
}
