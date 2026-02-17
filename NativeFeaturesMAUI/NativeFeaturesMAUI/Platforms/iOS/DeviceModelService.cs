#if IOS || MACCATALYST
using UIKit;

namespace NativeFeaturesMAUI;

public static partial class DeviceModelService
{
    internal static partial string PlatformModel() => UIDevice.CurrentDevice.Model ?? UIDevice.CurrentDevice.Name ?? "iOS device";
}

#endif
