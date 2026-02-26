using Microsoft.Extensions.Logging;

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

            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

#if ANDROID
            builder.Services.AddSingleton<IToastService, AndroidToastService>();
#elif IOS || MACCATALYST
            builder.Services.AddSingleton<IToastService, ToastService>();
#elif WINDOWS
            builder.Services.AddSingleton<IToastService, WindowsToastService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
