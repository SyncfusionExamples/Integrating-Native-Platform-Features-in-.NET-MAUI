#if ANDROID
using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Android implementation of <see cref="IToastService"/>.
    /// </summary>
    public sealed class AndroidToastService : IToastService
    {
        private const string ChannelId = "app.demo.high";
        private static bool _channelReady;

        /// <summary>
        /// Shows a simple notification with a title and message using NotificationCompat.
        /// Ensures the channel exists on Android 8.0+ before posting.
        /// </summary>
        /// <param name="message">The message to display. Null is treated as empty.</param>
        public async Task ShowAsync(string message)
        {
            var context = Android.App.Application.Context;
            if (context is null)
                return;

            EnsureChannel(context);
            var safeMessage = message ?? string.Empty;
            var builder = new NotificationCompat.Builder(context!, ChannelId)
                .SetContentTitle("Notification")
                .SetContentText(safeMessage)
                .SetSmallIcon(Android.Resource.Drawable.StatNotifyMore)
                .SetAutoCancel(true);
            var nmCompat = NotificationManagerCompat.From(context);
            var notification = builder.Build()!;
            if (nmCompat is not null)
            {
                nmCompat.Notify(Random.Shared.Next(), notification);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates the notification channel on Android 8.0+ (API 26+). Uses HIGH importance to allow heads-up alerts,
        /// subject to device and user settings. No-ops on older versions.
        /// </summary>
        /// <param name="ctx">Application context.</param>
        private static void EnsureChannel(Context ctx)
        {
            if (_channelReady)
            {
                return;
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var importance = NotificationImportance.High;
                var channel = new NotificationChannel(
                    ChannelId,
                    "Demo High",
                    importance
                );

                channel.Description = "Demo channel for visible notifications";
                var notify = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
                if (notify is not null)
                {
                    notify.CreateNotificationChannel(channel);
                }
            }

            _channelReady = true;
        }
    }
}
#endif