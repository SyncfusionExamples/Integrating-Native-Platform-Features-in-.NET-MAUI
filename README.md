# Integrating-Native-Platform-Features-in-.NET-MAUI
This demo shows how to access native capabilities camera, location, toasts, vibration, sensors in your .NET MAUI apps with simple examples and clear explanations.

## Overview


## Interface and Dependency Injection

### 1) Define interface in shared code

```csharp
// Services/IDeviceService.cs
public interface IDeviceService
{
    string GetDeviceModel();
}
```

### 2) Android implementation

```csharp
// Platforms/Android/DeviceService.cs
using Android.OS;

public class DeviceService : IDeviceService
{
    public string GetDeviceModel()
    {
        return Build.Model;
    }
}
```

### 3) iOS implementation

```csharp
// Platforms/iOS/DeviceService.cs
using UIKit;

public class DeviceService : IDeviceService
{
    public string GetDeviceModel()
    {
        return UIDevice.CurrentDevice.Model;
    }
}
```

### 4) Register service

```csharp
// MauiProgram.cs
builder.Services.AddSingleton<IDeviceService, DeviceService>();
```

### 5) Consume in UI or elsewhere

```csharp
public partial class MainPage : ContentPage
{
    public MainPage(IDeviceService deviceService)
    {
        InitializeComponent();
        deviceLabel.Text = deviceService.GetDeviceModel();
    }
}
```

## Access native APIs directly

Inside platform folders you can use native namespaces for more complex features.

### Android

```csharp
using Android.Hardware;
using Android.Content;
```

### iOS

```csharp
using CoreLocation;
using Foundation;
```

## Handling permissions

### Android

Edit `Platforms/Android/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.CAMERA" />
```

Add other permissions as needed per feature.

### iOS

Edit `Platforms/iOS/Info.plist`:

```xml
<key>NSCameraUsageDescription</key>
<string>Camera access is required.</string>
```

Add usage descriptions for each required capability.

## Integrating native SDKs

### Android

1. Add `.aar` or `.jar` file under `Platforms/Android`.
2. Configure `MainActivity.cs` or Gradle settings if required.
3. Wrap SDK calls via an interface to keep shared code clean.

### iOS

1. Add `.framework` under `Platforms/iOS`.
2. Configure `AppDelegate.cs` or project settings as needed.
3. Expose SDK functionality through shared interfaces or services.

## Conditional compilation

When small platform‑specific branches are needed, use directives:

```csharp
#if ANDROID
    // Android logic
#elif IOS
    // iOS logic
#endif
```

Use sparingly; prefer the interface or partial class patterns for larger features.

## Best practices

* Keep business logic in shared code; isolate native code in platform folders.
* Use interfaces or partial classes to avoid duplication.
* Prefer built‑in cross‑platform APIs first; add native code only when unavoidable.
* Test on real devices for each platform.
* Handle runtime permissions carefully and verify manifest or plist entries.
* Register handler customizations early in startup.

## Troubleshooting

| Issue | Action |
| ---------------------------------- | --------------------------------------------------- |
| Build error | Clean and rebuild solution. |
| Permission denied at runtime | Verify manifest or Info.plist entries. |
| Native namespace or type not found | Ensure code is in correct platform folder. |
| Handler customization not applied | Confirm registration order; place before app build. |

## Conclusion

We hope this guide gave you a clearer picture of how native features can elevate your .NET MAUI applications. With simple APIs you can build apps that feel instantly responsive and naturally integrated with every device they run on. These capabilities help your application behave smarter, react faster, and deliver experiences that truly feel native on Android, iOS, Windows, and macOS.

For current Syncfusion customers, the newest version of Essential Studio is available from the [license and downloads page](https://www.syncfusion.com/Account/Login?ReturnUrl=%2faccount%2fdownloads). If you are not yet a customer, you can try our 30-day free [trial](https://www.syncfusion.com/downloads) to check out these new features. 
 For questions, you can contact us through our support [forums](https://www.syncfusion.com/forums), [feedback portal](https://www.syncfusion.com/feedback), or support [portal](https://support.syncfusion.com/). We are always happy to assist you!

