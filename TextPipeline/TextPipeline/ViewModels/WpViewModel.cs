using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Domain;
using TextPipeline;
using WpEngine.Core;
using WpEngine.Models;

namespace TextPipeline.ViewModels;

/// <summary>
/// ViewModel вкладки WP (Weakest Precondition).
/// Связывает пользовательский ввод с Domain и предоставляет результаты вычисления.
/// Использует существующие ObservableObject и RelayCommand проекта.
/// </summary>
public sealed class WpViewModel : ObservableObject
{
    private readonly WpEngineService _engine = new();

    private string _programText = "";
    private string _postcondition = "";
    private string _precondition = "";
    private string _hoareTriple = "";
    private string _errorMessage = "";
    private string _stepsText = "";
    private string _definednessConditionsText = "";
    private string _variablesText = "";
    private string _assignmentsText = "";
    private string _conditionsText = "";
    private int _statementCount;
    private int _branchCount;
    private bool _isSuccess;
    private WpExample? _selectedExample;

    /// <summary>Текст программы, для которой вычисляется слабейшее предусловие.</summary>
    public string ProgramText
    {
        get => _programText;
        set => Set(ref _programText, value);
    }

    /// <summary>Требуемое постусловие Q.</summary>
    public string Postcondition
    {
        get => _postcondition;
        set => Set(ref _postcondition, value);
    }

    /// <summary>Вычисленное слабейшее предусловие wp(S, Q).</summary>
    public string Precondition
    {
        get => _precondition;
        private set => Set(ref _precondition, value);
    }

    /// <summary>Представление тройки Хоара.</summary>
    public string HoareTriple
    {
        get => _hoareTriple;
        private set => Set(ref _hoareTriple, value);
    }

    /// <summary>Сообщение об ошибке при разборе или вычислении.</summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => Set(ref _errorMessage, value);
    }

    /// <summary>Шаги вычисления предусловия, выведенные Domain.</summary>
    public string StepsText
    {
        get => _stepsText;
        private set => Set(ref _stepsText, value);
    }

    /// <summary>Условия определённости выражений.</summary>
    public string DefinednessConditionsText
    {
        get => _definednessConditionsText;
        private set => Set(ref _definednessConditionsText, value);
    }

    /// <summary>Переменные, найденные в программе.</summary>
    public string VariablesText
    {
        get => _variablesText;
        private set => Set(ref _variablesText, value);
    }

    /// <summary>Операторы присваивания, найденные в программе.</summary>
    public string AssignmentsText
    {
        get => _assignmentsText;
        private set => Set(ref _assignmentsText, value);
    }

    /// <summary>Условия ветвления, найденные в программе.</summary>
    public string ConditionsText
    {
        get => _conditionsText;
        private set => Set(ref _conditionsText, value);
    }

    public int StatementCount
    {
        get => _statementCount;
        private set => Set(ref _statementCount, value);
    }

    public int BranchCount
    {
        get => _branchCount;
        private set => Set(ref _branchCount, value);
    }

    /// <summary>Признак успешного вычисления.</summary>
    public bool IsSuccess
    {
        get => _isSuccess;
        private set => Set(ref _isSuccess, value);
    }

    /// <summary>Готовые примеры, предоставляемые Domain.</summary>
    public ObservableCollection<WpExample> Examples { get; }

    /// <summary>Выбранный пример из списка.</summary>
    public WpExample? SelectedExample
    {
        get => _selectedExample;
        set
        {
            if (Set(ref _selectedExample, value))
            {
                if (value is not null)
                    LoadExample();

                LoadExampleCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Команда вычисления предусловия и анализа программы.</summary>
    public RelayCommand CalculateCommand { get; }

    /// <summary>Команда загрузки выбранного примера в поля ввода.</summary>
    public RelayCommand LoadExampleCommand { get; }

    public WpViewModel()
    {
        Examples = new ObservableCollection<WpExample>(_engine.GetExamples());
        CalculateCommand = new RelayCommand(Calculate);
        LoadExampleCommand = new RelayCommand(LoadExample, () => SelectedExample is not null);
    }

    /// <summary>Переносит текст программы и постусловие выбранного примера в поля ввода.</summary>
    public void LoadExample()
    {
        if (SelectedExample is null)
            return;

        ProgramText = SelectedExample.Program;
        Postcondition = SelectedExample.Postcondition;
        (LoadExampleCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>Вызывает Domain-сервис и публикует результаты для UI.</summary>
    public void Calculate()
    {
        WpExecutionResult result = _engine.Execute(ProgramText, Postcondition);
        WpResult calculation = result.Calculation;

        IsSuccess = calculation.Success;
        Precondition = calculation.Precondition;
        HoareTriple = calculation.HoareTriple;
        ErrorMessage = calculation.Error ?? "";

        StepsText = string.Join(Environment.NewLine, calculation.Steps);
        DefinednessConditionsText = string.Join(Environment.NewLine, calculation.DefinednessConditions);
        StatementCount = result.StatementCount;
        BranchCount = result.BranchCount;
        VariablesText = string.Join(", ", result.Variables);
        AssignmentsText = string.Join(Environment.NewLine, result.Assignments);
        ConditionsText = string.Join(Environment.NewLine, result.Conditions);
    }
}
