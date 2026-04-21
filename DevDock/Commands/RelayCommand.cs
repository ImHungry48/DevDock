using System;
using System.Windows.Input;

namespace DevDock.Commands
{
    /// <summary>
    /// Basic ICommand implementation used to bind ViewModel actions to WPF controls.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Delegate that contains the command's actual action.
        private readonly Action<object?> _execute;

        // Optional delegate that determines whether the command can currently run.
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            // WPF re-queries command availability automatically during relevant UI changes.
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            // If no guard is provided, the command is always executable.
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
