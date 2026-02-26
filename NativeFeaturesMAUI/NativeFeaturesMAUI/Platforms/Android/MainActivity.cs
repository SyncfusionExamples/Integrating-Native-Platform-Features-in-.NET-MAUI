using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace NativeFeaturesMAUI
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Android 13+ (API 33) requires runtime POST_NOTIFICATIONS permission
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications)
                    != (int)Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this,
                        new[] { Manifest.Permission.PostNotifications }, 1001);
                }
            }
        }
    }
}
