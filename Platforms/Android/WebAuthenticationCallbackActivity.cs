using Android.App;
using Android.Content.PM;

namespace RoutineKeeper;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Android.Content.Intent.ActionView },
              Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
              DataScheme = "com.googleusercontent.apps.320661239781-12o220t3ji9dkvt3i7029giugfpqti6q")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
