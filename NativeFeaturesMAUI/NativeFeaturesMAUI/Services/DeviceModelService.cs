namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Provides a cross-platform abstraction for retrieving a user-friendly
    /// device model string. This single-file implementation contains all
    /// platform-specific code via conditional compilation, so platform
    /// DeviceModelService.cs files are not required.
    /// </summary>
    public static class DeviceModelService
    {
        /// <summary>
        /// Returns the device model by invoking the platform-specific implementation.
        /// </summary>
        public static string Model() => PlatformModel();

        /// <summary>
        /// Platform-specific implementation for retrieving the device model.
        /// Implementations are selected via conditional compilation.
        /// </summary>
        internal static string PlatformModel()
        {
#if ANDROID
            return global::Android.OS.Build.Model ?? global::Android.OS.Build.Device ?? "Android device";
#elif IOS || MACCATALYST
            return global::UIKit.UIDevice.CurrentDevice.Model ?? global::UIKit.UIDevice.CurrentDevice.Name ?? "iOS device";
#elif WINDOWS
            return System.Environment.MachineName ?? "Windows device";
#else
            return "Unknown device";
#endif
        }
    }
}