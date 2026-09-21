using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace TextPipeline
{
    /// <summary>
    /// Главная ViewModel приложения.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        /// <summary>
        /// Коллекция всех операций
        /// </summary>
        public ObservableCollection<OperationViewModelBase> Operations { get; }

        /// <summary>
        /// Выбранная операция
        /// </summary>
        private OperationViewModelBase _selectedOperation;

        /// <summary>
        /// Текущая операция, выбранная в ListBox
        /// </summary>
        public OperationViewModelBase SelectedOperation
        {
            get => _selectedOperation;
            set => Set(ref _selectedOperation, value);
        }

        /// <summary>
        /// Создание списка из трёх операций; выбор первой (Нормализация) по умолчанию, чтобы карточка справа не была пустой при запуске
        /// </summary>
        public MainViewModel()
        {
            Operations = new ObservableCollection<OperationViewModelBase>
            {
                new NormalizeViewModel(),
                new FilterViewModel(),
                new MaskViewModel()
            };

            //По умолчанию — первая операция
            SelectedOperation = Operations.First();
        }
    }
}
