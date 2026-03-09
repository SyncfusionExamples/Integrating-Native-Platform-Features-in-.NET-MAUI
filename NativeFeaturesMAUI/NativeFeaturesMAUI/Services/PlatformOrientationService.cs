namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Provides a simple, cross-platform way to retrieve the current device orientation
    /// as a readable string. Each platform uses a lightweight API to infer portrait/landscape.
    /// </summary>
    public static class PlatformOrientationService
    {
        /// <summary>
        /// Returns a human-friendly orientation string based on the current platform.
        /// Android: infers from the default display rotation.
        /// iOS/MacCatalyst: uses the application's status bar orientation.
        /// Windows: returns a placeholder string (can be extended via DisplayInformation).
        /// Other: returns "Unknown".
        /// </summary>
        public static string GetOrientation()
        {
#if ANDROID
            var wm = Android.App.Application.Context.GetSystemService(Android.Content.Context.WindowService)
                     as Android.Views.IWindowManager;
            var rotation = wm?.DefaultDisplay?.Rotation ?? Android.Views.SurfaceOrientation.Rotation0;
            bool landscape = rotation == Android.Views.SurfaceOrientation.Rotation90
                             || rotation == Android.Views.SurfaceOrientation.Rotation270;

            return landscape ? "Landscape (Android)" : "Portrait (Android)";

#elif IOS || MACCATALYST
            // NOTE: StatusBarOrientation is simple and works in most app scenarios,
            // but view/window-based orientation queries are preferred in modern UIKit scenes.
            var orientation = UIKit.UIApplication.SharedApplication.StatusBarOrientation;

            // Portrait if upright or upside-down; otherwise landscape.
            bool portrait = orientation == UIKit.UIInterfaceOrientation.Portrait
                            || orientation == UIKit.UIInterfaceOrientation.PortraitUpsideDown;

            return portrait ? "Portrait (iOS/macOS)" : "Landscape (iOS/macOS)";

#elif WINDOWS
            // Simplified: Windows has orientation APIs via DisplayInformation if you need exact values.
            // You can enhance this by referencing Windows.Graphics.Display.DisplayInformation.
            return "Orientation detection sample (Windows)";
#else
            // Fallback for unknown/unsupported platforms.
            return "Unknown";
#endif
        }
    }
}