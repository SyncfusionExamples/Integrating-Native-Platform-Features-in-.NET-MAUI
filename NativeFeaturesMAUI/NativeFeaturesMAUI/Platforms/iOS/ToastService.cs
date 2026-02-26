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
        /// <param name="message">The text to display inside the toast.</param>
        public Task ShowAsync(string message)
        {
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

                    if (window == null)
                    {
                        return;
                    }

                    var safeBottom = window.SafeAreaInsets.Bottom;
                    var width = window.Frame.Width - 40;
                    var height = 60;
                    var x = 20;
                    var y = window.Frame.Height - height - 40 - safeBottom;

                    // Create the toast label view.
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
                        Task.Delay(1800).ContinueWith(_ =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                UIView.Animate(0.25, () => toastLabel.Alpha = 0f, () => toastLabel.RemoveFromSuperview());
                            });
                        });
                    });
                });
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToastService iOS] ShowAsync failed: {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
#endif