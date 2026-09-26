using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace TextPipeline
{
    /// <summary>
    /// ViewModel для операции «Фильтрация по типу символов»
    /// </summary>
    public class FilterViewModel : OperationViewModelBase
    {
        /// <summary>
        /// Имя, отображаемое в ListBox и заголовке карточки
        /// </summary>
        public override string DisplayName => "Фильтрация по типу символов";

        /// <summary>
        /// Контракт из Domain.Contracts.Filter
        /// </summary>
        public override OperationContract Contract => Contracts.Filter;

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

        private CharacterType _selectedTypes = CharacterType.Letters;

        /// <summary>
        /// Текущий набор выбранных типов
        /// </summary>
        public CharacterType SelectedTypes
        {
            get => _selectedTypes;
            set
            {
                if (Set(ref _selectedTypes, value))
                    CheckPre();
            }
        }

        private bool _filterLetters = true;

        /// <summary>
        /// Привязка к CheckBox «Буквы»
        /// </summary>
        public bool FilterLetters
        {
            get => _filterLetters;
            set
            {
                if (Set(ref _filterLetters, value))
                    UpdateFlag(CharacterType.Letters, value);
            }
        }

        private bool _filterDigits;

        /// <summary>
        /// Привязка к CheckBox «Цифры»
        /// </summary>
        public bool FilterDigits
        {
            get => _filterDigits;
            set
            {
                if (Set(ref _filterDigits, value))
                    UpdateFlag(CharacterType.Digits, value);
            }
        }

        private bool _filterOthers;

        /// <summary>
        /// Привязка к CheckBox «Прочие»
        /// </summary>
        public bool FilterOthers
        {
            get => _filterOthers;
            set
            {
                if (Set(ref _filterOthers, value))
                    UpdateFlag(CharacterType.Other, value);
            }
        }

        /// <summary>
        /// Выставляет или снимает отдельный флаг в SelectedTypes. После изменения — пересчёт предусловия
        /// </summary>
        private void UpdateFlag(CharacterType flag, bool setOn)
        {
            if (setOn)
                SelectedTypes |= flag;//ИЛИ с присваиванием - ставит нужный бит enum на 1 и не трогает остальные
            else
                SelectedTypes &= ~flag;//И с присваиванием с инверсией - зануление выбранного бита enum
        }

        /// <summary>
        /// Создание команду «Выполнить»
        /// </summary>
        public FilterViewModel()
        {
            ExecuteCommand = new RelayCommand(Execute, () => IsPreSatisfied);
        }

        /// <summary>
        /// Предусловие: строка не null И выбран хотя бы один тип
        /// </summary>
        public override void CheckPre()
        {
            IsPreSatisfied = InputText != null && SelectedTypes != CharacterType.None;

            (ExecuteCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Вызов операции фильтрации по типу символов
        /// </summary>
        public override void Execute()
        {
            try
            {
                //Вызов логики в Domain
                ResultText = TextOperations.FilterByCharType(InputText, SelectedTypes);
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
