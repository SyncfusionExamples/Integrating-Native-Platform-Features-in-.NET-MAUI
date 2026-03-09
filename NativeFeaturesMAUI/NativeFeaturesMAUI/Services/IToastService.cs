using System.Threading.Tasks;

namespace NativeFeaturesMAUI
{
    /// <summary>
    /// Provides functionality to display toast notifications.
    /// </summary>
    public interface IToastService
    {
        void Show(string message);
    }
}
