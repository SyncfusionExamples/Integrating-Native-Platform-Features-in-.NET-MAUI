#if MACCATALYST
using UIKit;

namespace NativeFeaturesMAUI;

/// <summary>
/// DeviceModelService implementation for Mac Catalyst platform, providing functionality to retrieve the device model information specific to Mac devices running Catalyst.
/// </summary>
public static partial class DeviceModelService
{
    /// <summary>
    /// Platform-specific implementation to retrieve the device model information on Mac Catalyst devices. It returns the device model or name as the device model, or a default string if both are unavailable.
    /// </summary>
    internal static partial string PlatformModel() => UIDevice.CurrentDevice.Model ?? UIDevice.CurrentDevice.Name ?? "Mac device";
}

#endif
