#if ANDROID
using Android.OS;

namespace NativeFeaturesMAUI;

/// <summary>
/// Provides platform-specific functionality for retrieving the device model information on Android devices.
/// </summary>
public static partial class DeviceModelService
{
    /// <summary>
    /// Platform-specific implementation to retrieve the device model information on Android devices. It returns the device model or name as the device model, or a default string if both are unavailable.
    /// </summary>
    /// </summary>
    internal static partial string PlatformModel() => Build.Model ?? Build.Device ?? "Android device";
}

#endif
