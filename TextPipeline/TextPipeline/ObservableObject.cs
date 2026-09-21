using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TextPipeline
{
    /// <summary>
    /// Базовый класс для всех ViewModel.
    /// Реализует автоматическое обновление при изменении свойств.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие для привязки. При срабатывании UI перечитывает значения привязанных свойств
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызывает событие изменения свойств
        /// </summary>
        /// <param name="name">Имя свойства (автоопределяется).</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Функция для обращения к событию изменения свойств в случае изменения значений
        /// </summary>
        /// <typeparam name="T"> Тип свойства </typeparam>
        /// <param name="field"> Ссылка на поле </param>
        /// <param name="value"> Новое значение </param>
        /// <param name="name"> Имя свойства (автоопределяется) </param>
        /// <returns> true, если значение обновлено; false, если было таким же </returns>
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
