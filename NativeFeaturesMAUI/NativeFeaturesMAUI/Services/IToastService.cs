using System.Threading.Tasks;

namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Provides functionality to display toast notifications asynchronously.
    /// </summary>
    public interface IToastService
    {
        Task ShowAsync(string message);
    }
}
