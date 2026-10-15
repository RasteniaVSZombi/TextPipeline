using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpEngine.Models;

namespace TextPipeline.ViewModels
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

        private WpViewModel _wpViewModel;
        public WpViewModel WpViewModel { get => _wpViewModel; set => Set(ref _wpViewModel, value); }

        /// <summary>
        /// Создание списка из трёх операций
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

            WpViewModel = new WpViewModel();
        }
    }
}
