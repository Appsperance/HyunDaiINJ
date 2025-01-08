using System;
using System.Windows.Input;

namespace HyunDaiINJ.ViewModels.Main
{
    internal class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            // Null 체크와 타입 캐스팅
            if (parameter is null)
            {
                return false;
            }

            if (parameter is T value)
            {
                return _canExecute?.Invoke(value) ?? true;
            }

            throw new ArgumentException($"Invalid parameter type. Expected: {typeof(T)}, Actual: {parameter?.GetType()}");
        }

        public void Execute(object? parameter)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null.");
            }

            if (parameter is T value)
            {
                Console.WriteLine($"RelayCommand Execute 호출: {parameter}"); // 디버깅 로그
                _execute(value);
            }
            else
            {
                throw new ArgumentException($"Invalid parameter type. Expected: {typeof(T)}, Actual: {parameter?.GetType()}");
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
