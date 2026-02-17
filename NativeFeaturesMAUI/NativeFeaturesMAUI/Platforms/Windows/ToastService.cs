#if WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Windows implementation of <see cref="IToastService"/> using Windows App SDK App Notifications.
    /// </summary>
    public sealed class WindowsToastService : IToastService
    {
        /// <summary>
        /// Shows a lightweight toast with a static title ("Notification") and the provided message text.
        /// </summary>
        /// <param name="message">The message body to display; null is treated as an empty string.</param>
        public Task ShowAsync(string message)
        {
            var safeMessage = message ?? string.Empty;
            var toast = new AppNotificationBuilder()
                .AddText("Notification")
                .AddText(safeMessage)
                .BuildNotification();
            AppNotificationManager.Default.Show(toast);

            return Task.CompletedTask;
        }
    }
}
#endif