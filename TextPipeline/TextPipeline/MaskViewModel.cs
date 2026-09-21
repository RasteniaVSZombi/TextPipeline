using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace TextPipeline
{
    /// <summary>
    /// ViewModel для операции «Применение маски-шаблона»
    /// </summary>
    public class MaskViewModel : OperationViewModelBase
    {
        /// <summary>
        /// Имя, отображаемое в ListBox и заголовке карточки
        /// </summary>
        public override string DisplayName => "Применение маски-шаблона";

        /// <summary>
        /// Контракт из Domain.Contracts.Mask
        /// </summary>
        public override OperationContract Contract => Contracts.Mask;


        private string _inputText = "";

        /// <summary>
        /// Входная строка. При изменении — пересчёт предусловия
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set
            {
                if (Set(ref _inputText, value))
                    CheckPre();
            }
        }

        private string _mask = "";

        /// <summary>
        /// Маска-шаблон, где "#" — место подстановки символа. При изменении — пересчёт предусловия
        /// </summary>
        public string Mask
        {
            get => _mask;
            set
            {
                if (Set(ref _mask, value))
                    CheckPre();
            }
        }

        /// <summary>
        /// Создание команды «Выполнить»
        /// </summary>
        public MaskViewModel()
        {
            ExecuteCommand = new RelayCommand(Execute, () => IsPreSatisfied);
        }

        /// <summary>
        /// Предусловия:
        /// 1) маска не null и содержит хотя бы один '#';
        /// 2) входная строка не null и без пробельных символов;
        /// 3) символов во входной строке не меньше, чем '#' в маске.
        /// </summary>
        public override void CheckPre()
        {
            //Маска существует и содержит '#'.
            var maskOk = Mask != null && Mask.Contains('#');

            //Входная строка существует и без пробелов.
            var inputOk = InputText != null && InputText.All(c => !char.IsWhiteSpace(c));

            //Символов хватает на все '#'.
            var enough = maskOk && inputOk && InputText.Length >= Mask.Count(c => c == '#');

            IsPreSatisfied = maskOk && inputOk && enough;

            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Вызов операции применения маски-шаблона
        /// </summary>
        public override void Execute()
        {
            try
            {
                //Вызов логики в Domain
                ResultText = TextOperations.ApplyMask(InputText, Mask);
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
