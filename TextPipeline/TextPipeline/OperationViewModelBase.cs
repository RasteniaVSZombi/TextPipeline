using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain;

namespace TextPipeline
{
    /// <summary>
    /// Базовый класс для ViewModel всех трёх операций. Содержит общую инфраструктуру. Каждая конкретная операция наследует этот класс
    /// </summary>
    public abstract class OperationViewModelBase : ObservableObject
    {
        /// <summary>
        /// Отображаемое имя операции
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Контракт операции
        /// </summary>
        public abstract OperationContract Contract { get; }

        #region Индикаторы

        /// <summary>
        /// Индикатор предусловия
        /// </summary>
        private bool _isPreSatisfied;

        /// <summary>
        /// true — предусловие выполнено на текущем вводе (зелёный).
        /// false — не выполнено (красный).
        /// </summary>
        public bool IsPreSatisfied
        {
            get => _isPreSatisfied;
            set => Set(ref _isPreSatisfied, value);
        }

        /// <summary>
        /// Индикатор постусловия
        /// </summary>
        private bool _isPostSatisfied;

        /// <summary>
        /// true — постусловие выполнено после запуска (зелёный).
        /// false — не выполнено или операция ещё не запускалась (красный).
        /// </summary>
        public bool IsPostSatisfied
        {
            get => _isPostSatisfied;
            set => Set(ref _isPostSatisfied, value);
        }


        /// <summary>
        /// Результат операции
        /// </summary>
        private string _resultText = "";

        /// <summary>
        /// Текстовый результат операции
        /// </summary>
        public string ResultText
        {
            get => _resultText;
            set => Set(ref _resultText, value);
        }

        #endregion


        #region Команды

        /// <summary>
        /// Команда «Выполнить»
        /// </summary>
        public ICommand ExecuteCommand { get; protected set; }

        /// <summary>
        /// Команда «Показать контракт»
        /// </summary>
        public ICommand ShowContractCommand { get; }

        #endregion

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        protected OperationViewModelBase()
        {
            ShowContractCommand = new RelayCommand(() =>
            {
                //Создаём модальное окно и подключаем на него контракт.
                var window = new ContractWindow
                {
                    DataContext = Contract
                };
                window.ShowDialog(); //блокирует до закрытия
            });
        }

        #region Абстрактные методы (реализуются в наследниках)

        /// <summary>
        /// Проверка, выполнено ли предусловие на текущем вводе
        /// </summary>
        public abstract void CheckPre();

        /// <summary>
        /// Выполняет операцию и обновляет постусловие.
        /// </summary>
        public abstract void Execute();

        #endregion
    }
}
