#if WINDOWS
using System;

namespace NativeFeaturesMAUI;

/// <summary>
/// DeviceModelService implementation for Windows platform, providing functionality to retrieve the device model information specific to Windows devices.
/// </summary>
public static partial class DeviceModelService
{
    /// <summary>
    /// Platform-specific implementation to retrieve the device model information on Windows devices. It returns the machine name as the device model, or a default string if the machine name is unavailable.
    /// </summary>
    /// <returns></returns>
    internal static partial string PlatformModel() => Environment.MachineName ?? "Windows device";
}

#endif
