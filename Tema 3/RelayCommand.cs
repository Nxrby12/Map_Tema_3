using System;
using System.Windows.Input;

namespace Tema_3
{
    // Versiunea generică (pentru comenzi cu parametri, ex: AddToCart)
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute((T)parameter!);

        public void Execute(object? parameter) => _execute((T)parameter!);

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    // Versiunea simplă (pentru comenzi fără parametri, ex: PlaceOrder)
    public class RelayCommand : RelayCommand<object>
    {
        // Constructor cu 1 parametru (doar acțiunea)
        public RelayCommand(Action execute) : base(_ => execute()) { }

        // Constructor cu 2 parametri (acțiunea + condiția) - REZOLVĂ EROAREA CS1729
        public RelayCommand(Action execute, Func<bool> canExecute)
            : base(_ => execute(), _ => canExecute()) { }
    }
}