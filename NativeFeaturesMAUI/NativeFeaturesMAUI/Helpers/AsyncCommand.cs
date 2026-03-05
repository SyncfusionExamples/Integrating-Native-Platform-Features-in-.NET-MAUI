using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NativeFeaturesMAUI.Helpers
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter) => await ExecuteAsync();

        public async Task ExecuteAsync()
        {
            try
            {
                await _execute();
            }
            catch
            {
                // swallow; callers should surface errors through return values/events
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
