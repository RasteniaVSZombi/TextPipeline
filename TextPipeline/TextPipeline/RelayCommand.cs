using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TextPipeline
{
    /// <summary>
    /// Привязка кнопки к методам ViewModel без написания полноценной команды на каждое действие
    /// </summary>
    public class RelayCommand : ICommand
    {
        //Событие, которое выполнится при нажатии на кнопку
        private readonly Action _execute;

        // Доступна ли команда (кнопка Enabled/Disabled)
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Создаёт команду.
        /// </summary>
        /// <param name="execute"> Действие при выполнении команды </param>
        /// <param name="canExecute"> свойство доступности команды </param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Событие для обновления состояния кнопки
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Определяет, может ли команда выполняться прямо сейчас. При незаданном _canExecute всегда true
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Выполняет событие команды
        /// </summary>
        public void Execute(object parameter) => _execute();

        /// <summary>
        /// Принудительно уведомляет о том, что условие CanExecute могло измениться
        /// </summary>
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
