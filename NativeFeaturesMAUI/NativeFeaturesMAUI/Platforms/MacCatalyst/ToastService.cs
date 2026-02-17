#if MACCATALYST
using UIKit;
using CoreGraphics;

namespace NativeFeaturesMAUI
{
    /// <summary>
    /// MacCatalyst implementation of <see cref="IToastService"/>.
    /// </summary>
    public class ToastService : IToastService
    {
        /// <summary>
        /// Shows a transient toast message using a UILabel attached to the active UIWindow.
        /// Ensures UI operations are dispatched on the main thread and honors the safe area.
        /// </summary>
        /// <param name="message">The text content to display in the toast.</param>
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
            catch
            {
                // ignore
            }

            return Task.CompletedTask;
        }
    }
}
#endif