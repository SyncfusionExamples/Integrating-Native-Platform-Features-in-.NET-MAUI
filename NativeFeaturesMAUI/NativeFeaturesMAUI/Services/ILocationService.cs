using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace NativeFeaturesMAUI.Services
{
    /// <summary>
    /// Defines a service for asynchronously retrieving the current location information.
    /// </summary>
    public interface ILocationService
    {
        Task<Location?> GetLocationAsync();
    }
}
