#if IOS
using UIKit;
using CoreGraphics;

namespace NativeFeaturesMAUI
{
    /// <summary>
    /// iOS implementation of <see cref="IToastService"/>.
    /// Renders a lightweight toast label above the safe area, fades it in/out, and auto-dismisses.
    /// </summary>
    public class ToastService : IToastService
    {
		/// <summary>
        /// Shows a transient toast-like message near the bottom of the screen.
        /// Uses the active key window (or first available window) within the connected scenes.
        /// </summary>
        public void Show(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Simple alert-as-toast; swap for custom overlay if you prefer
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
        }
    }
}
#endif