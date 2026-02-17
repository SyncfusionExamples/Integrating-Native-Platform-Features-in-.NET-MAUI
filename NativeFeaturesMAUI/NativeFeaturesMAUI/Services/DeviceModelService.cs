namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Provides a cross-platform abstraction for retrieving a user-friendly
    /// device model string.
    /// </summary>
    public static partial class DeviceModelService
    {
        /// <summary>
        /// Returns the device model by invoking the platform-specific implementation.
        /// </summary>
        public static string Model() => PlatformModel();

        /// <summary>
        /// Platform-specific implementation for retrieving the device model.
        /// Must be implemented in each target framework using partial methods.
        /// </summary>
        internal static partial string PlatformModel();

#if !ANDROID && !IOS && !MACCATALYST && !WINDOWS
        /// <summary>
        /// Fallback implementation used when running on unsupported or unknown platforms.
        /// </summary>
        /// <returns>A generic device model string ("Unknown device").</returns>
        internal static partial string PlatformModel() => "Unknown device";
#endif
    }
}