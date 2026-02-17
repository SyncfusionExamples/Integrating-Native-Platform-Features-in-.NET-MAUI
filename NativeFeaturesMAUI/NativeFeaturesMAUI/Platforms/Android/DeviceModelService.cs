#if ANDROID
using Android.OS;

namespace NativeFeaturesMAUI;

public static partial class DeviceModelService
{
    internal static partial string PlatformModel() => Build.Model ?? Build.Device ?? "Android device";
}

#endif
