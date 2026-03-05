using System;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

namespace NativeFeaturesMAUI.Services
{
    /// <summary>
    /// Location service implementation that requests permission and retrieves the current location using .NET MAUI Essentials APIs.
    /// </summary>
    public class LocationService : ILocationService
    {
        /// <summary>
        /// Gets the current location asynchronously. It first requests permission to access location data, and if granted, it retrieves the location with medium accuracy and a timeout of 10 seconds. If permission is denied, it returns null.
        /// </summary>
        /// <returns></returns>
        public async Task<Location?> GetLocationAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            return await Geolocation.GetLocationAsync(request);
        }
    }
}
