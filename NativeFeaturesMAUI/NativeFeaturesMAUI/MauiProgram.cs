using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;
using Microsoft.Extensions.DependencyInjection;
using NativeFeaturesMAUI;
using NativeFeaturesMAUI.Services;


namespace NativeFeaturesMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IConnectivity>(_ => Connectivity.Current);
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<MainPageViewModel>();

#if ANDROID
            builder.Services.AddSingleton<IToastService, AndroidToastService>();
#elif IOS
            builder.Services.AddSingleton<IToastService, ToastService>();
#elif MACCATALYST
            builder.Services.AddSingleton<IToastService, ToastService>();
#elif WINDOWS
            builder.Services.AddSingleton<IToastService, WindowsToastService>();
#else
            builder.Services.AddSingleton<IToastService, NullToastService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    /// <summary>
    /// Optional: a no-op implementation used as a safe fallback.
    /// </summary>
    public sealed class NullToastService : IToastService
    {
        public void Show(string message) {}
    }
}