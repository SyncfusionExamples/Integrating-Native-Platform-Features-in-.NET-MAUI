#if WINDOWS
using System;

namespace NativeFeaturesMAUI;

public static partial class DeviceModelService
{
    internal static partial string PlatformModel() => Environment.MachineName ?? "Windows device";
}

#endif
