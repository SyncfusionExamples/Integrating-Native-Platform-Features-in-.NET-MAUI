#if ANDROID
using Android.Widget;
using NativeFeaturesMAUI;

namespace NativeFeaturesMAUI;

public sealed class AndroidToastService : IToastService
{
    public void Show(string message)
    {
        var safe = message ?? string.Empty;

        // Ensure UI thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var ctx = Android.App.Application.Context;
            if (ctx is null) return;

            Toast.MakeText(ctx, safe, ToastLength.Short)?.Show();
        });
    }
}
#endif