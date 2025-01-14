using System;
using System.Windows.Input;

namespace HyunDaiINJ.ViewModels.Main
{
    /// <summary>
    /// 매개변수 없는 동기 액션용 커맨드
    /// </summary>
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

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
            => _execute();

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 매개변수 있는 동기 액션용 커맨드
    /// </summary>
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
            if (parameter is T value)
                return _canExecute?.Invoke(value) ?? true;

            // 인자가 null인데 T가 값 형식인 경우 등 처리
            if (parameter == null && default(T) == null)
                return _canExecute?.Invoke(default!) ?? true;

            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T value)
            {
                _execute(value);
            }
            else if (parameter == null && default(T) == null)
            {
                _execute(default!);
            }
        }

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
