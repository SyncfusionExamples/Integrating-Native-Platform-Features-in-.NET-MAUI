namespace NativeFeaturesMAUI
{
#if MACCATALYST
using CoreGraphics;
using UIKit;
using System.Linq;

#elif IOS
using UIKit;
using System.Linq;

#elif ANDROID
    using Android.Widget;

#elif WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

#endif
    /// <summary>
    /// ToastService provides a simple, cross-platform way to display short notifications or "toasts" to the user. It abstracts away platform-specific APIs to offer a unified interface for showing transient messages. The implementation uses native mechanisms on each platform to ensure a consistent user experience, while gracefully degrading on unsupported platforms.
    /// </summary>
    public sealed class ToastService : IToastService
    {
        /// <summary>
        /// Displays a short, platform-specific notification or toast message to the user.
        /// </summary>
        /// <remarks>The appearance and behavior of the notification may vary depending on the platform.
        /// On unsupported platforms, this method has no effect.</remarks>
        /// <param name="message">The message text to display in the notification. If null, an empty string is shown.</param>
        public void Show(string message)
        {
#if ANDROID
            var safe = message ?? string.Empty;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var ctx = Android.App.Application.Context;
                if (ctx is null) return;
                Toast.MakeText(ctx, safe, ToastLength.Short)?.Show();
            });
#elif IOS
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var window = UIApplication.SharedApplication?.KeyWindow
                             ?? UIApplication.SharedApplication?.ConnectedScenes?
                                  .OfType<UIWindowScene>()
                                  .SelectMany(s => s.Windows)
                                  .FirstOrDefault(w => w.IsKeyWindow);
                var root = window?.RootViewController;
                if (root == null) return;

                var alert = UIAlertController.Create(null, message ?? string.Empty, UIAlertControllerStyle.Alert);
                root.PresentViewController(alert, true, null);

                var delay = new CoreFoundation.DispatchTime(CoreFoundation.DispatchTime.Now, (long)(1.5 * 1_000_000_000));
                CoreFoundation.DispatchQueue.MainQueue.DispatchAfter(delay, () =>
                {
                    alert.DismissViewController(true, null);
                });
            });
#elif MACCATALYST
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var scenes = UIApplication.SharedApplication
                        .ConnectedScenes
                        .OfType<UIWindowScene>()
                        .ToArray();

                    var window = scenes.SelectMany(s => s.Windows)
                        .FirstOrDefault(w => w.IsKeyWindow)
                        ?? scenes.SelectMany(s => s.Windows).FirstOrDefault();

                    if (window == null) return;

                    var safeBottom = window.SafeAreaInsets.Bottom;

                    nfloat width = window.Frame.Width - 40;
                    nfloat height = 60;
                    nfloat x = 20;
                    nfloat y = window.Frame.Height - height - 40 - safeBottom;

                    var toastLabel = new UILabel(new CGRect(x, y, width, height))
                    {
                        Text = message,
                        TextColor = UIColor.White,
                        TextAlignment = UITextAlignment.Center,
                        Lines = 2,
                        Alpha = 0f,
                        BackgroundColor = UIColor.FromWhiteAlpha(0f, 0.8f),
                    };
                    toastLabel.Layer.CornerRadius = 10;
                    toastLabel.Layer.MasksToBounds = true;

                    window.AddSubview(toastLabel);

                    UIView.Animate(0.25, () => toastLabel.Alpha = 1.0f, () =>
                    {
                        var delay = new CoreFoundation.DispatchTime(CoreFoundation.DispatchTime.Now,
                            (long)(1.8 * 1_000_000_000));

                        CoreFoundation.DispatchQueue.MainQueue.DispatchAfter(delay, () =>
                        {
                            UIView.Animate(0.25, () => toastLabel.Alpha = 0f,
                                () => toastLabel.RemoveFromSuperview());
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToastService MAC] Show failed: {ex}");
            }
#elif WINDOWS
            var safeMessage = message ?? string.Empty;

            var toast = new AppNotificationBuilder()
                .AddText("Notification")
                .AddText(safeMessage)
                .BuildNotification();

            AppNotificationManager.Default.Show(toast);
#endif
        }
    }
}
