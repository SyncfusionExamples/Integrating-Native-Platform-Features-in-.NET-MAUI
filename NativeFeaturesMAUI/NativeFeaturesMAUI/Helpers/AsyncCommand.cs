using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NativeFeaturesMAUI.Helpers
{
    /// <summary>
    /// Asynchronous command implementation for use in MVVM patterns, allowing for async operations without blocking the UI thread.
    /// </summary>
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        public event EventHandler? CanExecuteChanged;

        public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Executes the command if it can be executed. This method is called by the UI to determine if the command can be executed and to execute it when triggered. It checks the CanExecute condition before executing the asynchronous operation.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter) => await ExecuteAsync();

        /// <summary>
        /// Executes the asynchronous command, handling any exceptions that may occur during execution. This method is called by the Execute method and can also be called directly if needed.
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            try
            {
                await _execute();
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event to notify the UI that the command's ability to execute may have changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
