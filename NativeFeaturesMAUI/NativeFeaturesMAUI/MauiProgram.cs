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
            builder.Services.AddSingleton<IToastService, ToastService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}