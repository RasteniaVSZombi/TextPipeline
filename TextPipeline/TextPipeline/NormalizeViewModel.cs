using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace TextPipeline
{
    /// <summary>
    /// ViewModel для операции «Нормализация»
    /// </summary>
    public class NormalizeViewModel : OperationViewModelBase
    {
        /// <summary>
        /// Имя, отображаемое в ListBox и заголовке карточки
        /// </summary>
        public override string DisplayName => "Нормализация";

        /// <summary>
        /// Контракт из Domain.Contracts.Normalize
        /// </summary>
        public override OperationContract Contract => Contracts.Normalize;

        private string _inputText = "";

        /// <summary>
        /// Входной текст. При изменении — пересчитывается Pre
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set
            {
                //Set возвращает true, если значение изменилось — перепроверяем предусловие.
                if (Set(ref _inputText, value))
                    CheckPre();
            }
        }

        /// <summary>
        /// Создание команды «Выполнить»
        /// </summary>
        public NormalizeViewModel()
        {
            ExecuteCommand = new RelayCommand(Execute, () => IsPreSatisfied);
        }

        /// <summary>
        /// Предусловие для нормализации: входная строка не null. Пустая строка — валидна, вернёт ""
        /// </summary>
        public override void CheckPre()
        {
            IsPreSatisfied = InputText != null;

            //Уведомляем команду, что доступность могла измениться.
            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Вызов операции нормализации
        /// </summary>
        public override void Execute()
        {
            try
            {
                //Вызов логики в Domain
                ResultText = TextOperations.Normalize(InputText);
                IsPostSatisfied = true;
            }
            catch
            {
                //Guard.Requires сработал в Domain
                IsPostSatisfied = false;
                ResultText = "Ошибка: предусловие не выполнено";
            }
        }
    }
}
