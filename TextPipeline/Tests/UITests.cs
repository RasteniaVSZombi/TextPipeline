using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using TextPipeline;
using TextPipeline.ViewModels;
using WpEngine.Core;
using WpEngine.Models;
using Xunit;

namespace Tests;

public class UITests
{
    #region Тесты первой вкладки


    [Fact]
    public void MainWindow_CanBeCreated()
    {
        // Проверяем успешное создание и закрытие главного окна приложения.
        var created = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            // Проверяем создание окна внутри WPF-потока.
            var result = window != null;

            // Закрываем окно в том же потоке, в котором оно было создано.
            window.Close();

            return result;
        });

        Assert.True(created);
    }

    [Fact]
    public void MainWindow_HasCorrectTitle()
    {
        // Проверяем заголовок главного окна.
        var title = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var result = window.Title;
            window.Close();

            return result;
        });

        Assert.Equal("Операции", title);
    }

    [Fact]
    public void MainWindow_ContainsOperationsList()
    {
        // Проверяем наличие списка операций на главном окне.
        var listBox = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            // Отображаем окно и обновляем его визуальное дерево.
            window.Show();
            window.UpdateLayout();

            // Ищем ListBox среди элементов интерфейса.
            var result = FindVisualChild<ListBox>(window);

            window.Close();

            return result;
        });

        Assert.NotNull(listBox);
    }

    [Fact]
    public void MainWindow_ContainsThreeOperations()
    {
        // Проверяем количество операций в списке.
        var count = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            // Создаём визуальное дерево окна перед поиском элементов.
            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Получаем количество элементов списка.
            var result = listBox?.Items.Count ?? 0;

            window.Close();

            return result;
        });

        Assert.Equal(3, count);
    }

    [Fact]
    public void MainWindow_ContainsContentControl()
    {
        // Проверяем наличие области отображения выбранной операции.
        var contentControl = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            // Ищем ContentControl в визуальном дереве окна.
            var result = FindVisualChild<ContentControl>(window);

            window.Close();

            return result;
        });

        Assert.NotNull(contentControl);
    }

    [Fact]
    public void MainWindow_FirstOperationCanBeSelected()
    {
        // Проверяем возможность выбора операции из списка.
        var selected = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Проверяем, что список существует и содержит элементы.
            if (listBox == null || listBox.Items.Count == 0)
            {
                window.Close();
                return false;
            }

            // Выбираем первую операцию в списке.
            listBox.SelectedIndex = 0;

            var result = listBox.SelectedItem != null;

            window.Close();

            return result;
        });

        Assert.True(selected);
    }

    [Fact]
    public void MainWindow_NormalizeOperationContainsExecuteButton()
    {
        // Проверяем наличие кнопки выполнения операции Normalize.
        var button = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Выбираем операцию Normalize.
            if (listBox != null)
            {
                listBox.SelectedIndex = 0;
            }

            window.UpdateLayout();

            // Ищем кнопку с текстом "Выполнить".
            var result = FindButtonByContent(window, "Выполнить");

            window.Close();

            return result;
        });

        Assert.NotNull(button);
    }

    [Fact]
    public void MainWindow_NormalizeOperationContainsContractButton()
    {
        // Проверяем наличие кнопки просмотра контракта.
        var button = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Выбираем операцию Normalize.
            if (listBox != null)
            {
                listBox.SelectedIndex = 0;
            }

            window.UpdateLayout();

            // Ищем кнопку отображения контракта.
            var result = FindButtonByContent(window, "Показать контракт");

            window.Close();

            return result;
        });

        Assert.NotNull(button);
    }

    [Fact]
    public void MainWindow_FilterOperationContainsCheckBoxes()
    {
        // Проверяем наличие элементов выбора типов символов.
        var checkBoxCount = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Выбираем операцию FilterByCharType.
            if (listBox != null)
            {
                listBox.SelectedIndex = 1;
            }

            window.UpdateLayout();

            // Считаем количество CheckBox в интерфейсе.
            var result = FindVisualChildren<CheckBox>(window).Count;

            window.Close();

            return result;
        });

        Assert.Equal(3, checkBoxCount);
    }

    [Fact]
    public void MainWindow_FilterOperationHasExpectedCheckBoxes()
    {
        // Проверяем названия вариантов фильтрации символов.
        var contents = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Выбираем операцию FilterByCharType.
            if (listBox != null)
            {
                listBox.SelectedIndex = 1;
            }

            window.UpdateLayout();

            // Получаем текст всех элементов CheckBox.
            var result = FindVisualChildren<CheckBox>(window)
                .ConvertAll(checkBox => checkBox.Content?.ToString() ?? string.Empty);

            window.Close();

            return result;
        });

        Assert.Contains("Буквы", contents);
        Assert.Contains("Цифры", contents);
        Assert.Contains("Прочие символы", contents);
    }

    [Fact]
    public void MainWindow_MaskOperationContainsExecuteButton()
    {
        // Проверяем наличие кнопки выполнения операции маски.
        var button = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Выбираем операцию ApplyMask.
            if (listBox != null)
            {
                listBox.SelectedIndex = 2;
            }

            window.UpdateLayout();

            // Ищем кнопку выполнения операции.
            var result = FindButtonByContent(window, "Выполнить");

            window.Close();

            return result;
        });

        Assert.NotNull(button);
    }

    [Fact]
    public void MainWindow_MaskOperationContainsTwoInputTextBoxes()
    {
        // Проверяем наличие полей ввода для текста и маски.
        var textBoxCount = RunOnUiThread(() =>
        {
            var window = new MainWindow();

            window.Show();
            window.UpdateLayout();

            var listBox = FindVisualChild<ListBox>(window);

            // Выбираем операцию ApplyMask.
            if (listBox != null)
            {
                listBox.SelectedIndex = 2;
            }

            window.UpdateLayout();

            // Считаем TextBox, отображаемые в выбранной операции.
            var result = FindVisualChildren<TextBox>(window).Count;

            window.Close();

            return result;
        });

        Assert.Equal(3, textBoxCount);
    }

    #endregion

    #region UI-тесты второй вкладки «Конструктор фрагмента»

    [Fact]
    public void FragmentConstructor_TabExists()
    {
        var header = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            window.Show();
            window.UpdateLayout();
            var tab = FindVisualChildren<TabItem>(window)
                .FirstOrDefault(item => item.Header?.ToString() == "Конструктор фрагмента");
            var result = tab?.Header?.ToString();
            window.Close();
            return result;
        });
        Assert.Equal("Конструктор фрагмента", header);
    }

    [Fact]
    public void FragmentConstructor_CanBeSelected()
    {
        var selected = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            window.Show();
            window.UpdateLayout();
            var tabs = FindVisualChild<TabControl>(window);
            if (tabs == null) { window.Close(); return false; }
            var tab = tabs.Items.OfType<TabItem>().FirstOrDefault(x => x.Header?.ToString() == "Конструктор фрагмента");
            if (tab == null) { window.Close(); return false; }
            tabs.SelectedItem = tab;
            window.UpdateLayout();
            var result = tabs.SelectedItem == tab;
            window.Close();
            return result;
        });
        Assert.True(selected);
    }

    [Fact]
    public void FragmentConstructor_ContainsExampleSelector()
    {
        var combo = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChild<ComboBox>(window);
            window.Close(); return result;
        });
        Assert.NotNull(combo);
    }

    [Fact]
    public void FragmentConstructor_ExampleSelectorIsPopulated()
    {
        var count = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChild<ComboBox>(window)?.Items.Count ?? 0;
            window.Close(); return result;
        });
        Assert.Equal(4, count);
    }

    [Fact]
    public void FragmentConstructor_ExampleSelectorDisplaysNames()
    {
        var displayMember = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChild<ComboBox>(window)?.DisplayMemberPath;
            window.Close(); return result;
        });
        Assert.Equal("Name", displayMember);
    }

    [Fact]
    public void FragmentConstructor_ContainsPostconditionGroup()
    {
        var exists = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<GroupBox>(window).Any(x => x.Header?.ToString() == "Цель (Post)");
            window.Close(); return result;
        });
        Assert.True(exists);
    }

    [Fact]
    public void FragmentConstructor_ContainsProgramEditorGroup()
    {
        var exists = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<GroupBox>(window).Any(x => x.Header?.ToString() == "Код фрагмента");
            window.Close(); return result;
        });
        Assert.True(exists);
    }

    [Fact]
    public void FragmentConstructor_ContainsCalculateButton()
    {
        var button = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindButtonByContent(window, "Рассчитать wp");
            window.Close(); return result;
        });
        Assert.NotNull(button);
    }

    [Fact]
    public void FragmentConstructor_ContainsShowTriadButton()
    {
        var button = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindButtonByContent(window, "Показать триаду");
            window.Close(); return result;
        });
        Assert.NotNull(button);
    }

    [Fact]
    public void FragmentConstructor_ContainsWeakestPreconditionOutput()
    {
        var exists = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<GroupBox>(window).Any(x => x.Header?.ToString() == "Слабейшее предусловие (wp)");
            window.Close(); return result;
        });
        Assert.True(exists);
    }

    [Fact]
    public void FragmentConstructor_ContainsHoareTriadOutput()
    {
        var exists = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<GroupBox>(window).Any(x => x.Header?.ToString() == "Триада Хоара");
            window.Close(); return result;
        });
        Assert.True(exists);
    }

    [Fact]
    public void FragmentConstructor_ContainsDefinednessConditionsOutput()
    {
        var exists = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<GroupBox>(window).Any(x => x.Header?.ToString() == "Условия определённости");
            window.Close(); return result;
        });
        Assert.True(exists);
    }

    [Fact]
    public void FragmentConstructor_ContainsProgramAndResultTextBoxes()
    {
        var count = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<TextBox>(window).Count;
            window.Close(); return result;
        });
        Assert.True(count >= 6);
    }

    [Fact]
    public void FragmentConstructor_ResultFieldsAreReadOnly()
    {
        var readOnlyCount = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindVisualChildren<TextBox>(window).Count(x => x.IsReadOnly);
            window.Close(); return result;
        });
        Assert.True(readOnlyCount >= 3);
    }

    [Fact]
    public void FragmentConstructor_ViewModelIsAttached()
    {
        var viewModelExists = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var result = (window.DataContext as TextPipeline.ViewModels.MainViewModel)?.WpViewModel != null;
            window.Close(); return result;
        });
        Assert.True(viewModelExists);
    }

    [Fact]
    public void FragmentConstructor_ExamplesAreLoadedInitially()
    {
        var data = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)?.WpViewModel;
            var result = (Count: vm?.Examples.Count ?? 0, Selected: vm?.SelectedExample != null);
            window.Close(); return result;
        });
        Assert.Equal(4, data.Count);
        Assert.True(data.Selected);
    }

    [Fact]
    public void FragmentConstructor_SelectingExampleLoadsProgramAndPostcondition()
    {
        var loaded = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            if (vm.Examples.Count < 2) { window.Close(); return false; }
            var example = vm.Examples[1];
            vm.SelectedExample = example;
            var result = vm.ProgramText == example.Program &&
                         vm.Postcondition == example.Postcondition &&
                         vm.PostconditionHuman == example.PostconditionDescription;
            window.Close(); return result;
        });
        Assert.True(loaded);
    }

    [Fact]
    public void FragmentConstructor_CalculateCommandIsAvailable()
    {
        var canExecute = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            var result = vm.CalculateCommand.CanExecute(null);
            window.Close(); return result;
        });
        Assert.True(canExecute);
    }

    [Fact]
    public void FragmentConstructor_CalculationOfDefaultExampleSucceeds()
    {
        var result = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            vm.ProgramText = "z := y - 4";
            vm.Postcondition = "z > 10";
            vm.Calculate();
            var data = (vm.IsSuccess, vm.Precondition, vm.ErrorMessage);
            window.Close(); return data;
        });
        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Precondition));
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage));
    }

    [Fact]
    public void FragmentConstructor_CalculationPopulatesAnalysisResults()
    {
        var result = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            vm.ProgramText = "z := y - 4";
            vm.Postcondition = "z > 10";
            vm.Calculate();
            var data = (vm.VariablesText, vm.AssignmentsText, vm.StepsText, vm.StatementCount);
            window.Close(); return data;
        });
        Assert.False(string.IsNullOrWhiteSpace(result.StepsText));
        Assert.True(result.StatementCount > 0);
    }

    [Fact]
    public void FragmentConstructor_ShowTriadButtonStartsDisabled()
    {
        var isEnabled = RunOnUiThread(() =>
        {
            var window = new MainWindow(); window.Show(); window.UpdateLayout();
            SelectFragmentTab(window); window.UpdateLayout();
            var result = FindButtonByContent(window, "Показать триаду")?.IsEnabled ?? false;
            window.Close(); return result;
        });
        Assert.False(isEnabled);
    }

    [Fact]
    public void FragmentConstructor_ShowTriadCommandCreatesHoareTriadAfterCalculation()
    {
        var triad = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            vm.ProgramText = "z := y - 4";
            vm.Postcondition = "z > 10";
            vm.Calculate();
            vm.ShowTriadCommand.Execute(null);
            var result = vm.TriadShort;
            window.Close(); return result;
        });
        Assert.Contains("Слабейшее предусловие", triad);
        Assert.Contains("Программа:", triad);
        Assert.Contains("Постусловие:", triad);
    }

    [Fact]
    public void FragmentConstructor_InvalidProgramShowsError()
    {
        var result = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            vm.ProgramText = "if (";
            vm.Postcondition = "x > 0";
            vm.Calculate();
            var data = (vm.IsSuccess, vm.ErrorMessage);
            window.Close(); return data;
        });
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void FragmentConstructor_InvalidCalculationDoesNotCreateTriad()
    {
        var triad = RunOnUiThread(() =>
        {
            var window = new MainWindow();
            var vm = (window.DataContext as TextPipeline.ViewModels.MainViewModel)!.WpViewModel;
            vm.ProgramText = "if (";
            vm.Postcondition = "x > 0";
            vm.Calculate();
            vm.ShowTriadCommand.Execute(null);
            var result = vm.TriadShort;
            window.Close(); return result;
        });
        Assert.Equal(string.Empty, triad);
    }

    private static void SelectFragmentTab(MainWindow window)
    {
        var tabs = FindVisualChild<TabControl>(window);
        if (tabs == null) return;
        var tab = tabs.Items.OfType<TabItem>()
            .FirstOrDefault(item => item.Header?.ToString() == "Конструктор фрагмента");
        if (tab != null) tabs.SelectedItem = tab;
    }

    #endregion


    #region Тесты окна контрактов
    [Fact]
    public void ContractWindow_CanBeCreated()
    {
        // Проверяем успешное создание и закрытие окна контракта.
        var created = RunOnUiThread(() =>
        {
            var window = new ContractWindow();

            // Проверяем создание окна внутри WPF-потока.
            var result = window != null;

            // Закрываем окно в том же потоке, в котором оно было создано.
            window.Close();

            return result;
        });

        Assert.True(created);
    }

    [Fact]
    public void ContractWindow_HasCorrectTitle()
    {
        // Проверяем заголовок окна контракта.
        var title = RunOnUiThread(() =>
        {
            var window = new ContractWindow();
            var result = window.Title;

            window.Close();

            return result;
        });

        Assert.Equal("Контракт операции", title);
    }

    [Fact]
    public void ContractWindow_ContainsCloseButton()
    {
        // Проверяем наличие кнопки закрытия окна контракта.
        var button = RunOnUiThread(() =>
        {
            var window = new ContractWindow();

            window.Show();
            window.UpdateLayout();

            // Ищем кнопку с текстом "Закрыть".
            var result = FindButtonByContent(window, "Закрыть");

            window.Close();

            return result;
        });

        Assert.NotNull(button);
    }

    [Fact]
    public void ContractWindow_ContainsContractTextBlock()
    {
        // Проверяем наличие элемента для отображения текста контракта.
        var textBlock = RunOnUiThread(() =>
        {
            var window = new ContractWindow();

            window.Show();
            window.UpdateLayout();

            // Ищем TextBlock в визуальном дереве окна.
            var result = FindVisualChild<TextBlock>(window);

            window.Close();

            return result;
        });

        Assert.NotNull(textBlock);
    }

    [Fact]
    public void ContractWindow_CanDisplayContractText()
    {
        // Проверяем возможность отображения текста контракта.
        var text = RunOnUiThread(() =>
        {
            var window = new ContractWindow();

            // Передаём тестовый текст в окно контракта.
            window.ContractContent = "Тестовый контракт";

            window.Show();
            window.UpdateLayout();

            // Получаем TextBlock и проверяем его содержимое.
            var textBlock = FindVisualChild<TextBlock>(window);
            var result = textBlock?.Text;

            window.Close();

            return result;
        });

        Assert.Equal("Тестовый контракт", text);
    }

    #endregion

    #region Вспомогательные методы

    private static T RunOnUiThread<T>(Func<T> action)
    {
        // Передаём действие в общий WPF-поток для безопасной работы с интерфейсом.
        return WpfTestHost.Run(action);
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        // Перебираем дочерние элементы визуального дерева.
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // Возвращаем найденный элемент нужного типа.
            if (child is T result)
            {
                return result;
            }

            // Рекурсивно ищем элемент среди вложенных компонентов.
            var descendant = FindVisualChild<T>(child);

            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static List<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        var result = new List<T>();

        // Обходим все дочерние элементы визуального дерева.
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // Добавляем найденный элемент нужного типа в список.
            if (child is T element)
            {
                result.Add(element);
            }

            // Продолжаем поиск во вложенных элементах.
            result.AddRange(FindVisualChildren<T>(child));
        }

        return result;
    }

    private static Button? FindButtonByContent(
        DependencyObject parent,
        string content)
    {
        // Получаем все кнопки из визуального дерева.
        foreach (var button in FindVisualChildren<Button>(parent))
        {
            // Ищем кнопку с заданным текстом.
            if (button.Content?.ToString() == content)
            {
                return button;
            }
        }

        return null;
    }

    private static class WpfTestHost
    {
        private static readonly object LockObject = new();
        private static Thread? _thread;
        private static Dispatcher? _dispatcher;
        private static Application? _application;

        public static T Run<T>(Func<T> action)
        {
            // Запускаем общий WPF-поток при первом обращении.
            EnsureStarted();

            T result = default!;
            Exception? exception = null;

            // Выполняем тестовое действие внутри WPF-потока.
            _dispatcher!.Invoke(() =>
            {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            // Передаём исключение обратно тесту, если оно возникло.
            if (exception != null)
            {
                throw exception;
            }

            return result;
        }

        private static void EnsureStarted()
        {
            // Если WPF-поток уже создан, повторно его не запускаем.
            if (_dispatcher != null)
            {
                return;
            }

            lock (LockObject)
            {
                // Повторно проверяем состояние после получения блокировки.
                if (_dispatcher != null)
                {
                    return;
                }

                var started = new ManualResetEventSlim(false);

                // Создаём отдельный STA-поток для выполнения WPF-кода.
                _thread = new Thread(() =>
                {
                    // Создаём единственный экземпляр WPF Application.
                    _application = new Application();

                    // Не завершаем Application при закрытии последнего тестового окна.
                    _application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    // Добавляем необходимые ресурсы интерфейса для тестовой среды.
                    // MainWindow.xaml обращается к BackgroundBrush уже на TabControl,
                    // поэтому ресурс должен быть зарегистрирован до создания окна.
                    _application.Resources["BackgroundBrush"] =
                        new SolidColorBrush(Color.FromRgb(255, 247, 235));

                    _application.Resources["BorderBrush"] =
                        new SolidColorBrush(Colors.LightGray);

                    _application.Resources["CardBackgroundBrush"] =
                        new SolidColorBrush(Colors.White);

                    _application.Resources["PrimaryButtonBrush"] =
                        new SolidColorBrush(Colors.SteelBlue);

                    _application.Resources["SuccessButtonBrush"] =
                        new SolidColorBrush(Colors.Green);

                    // Добавляем тестовые конвертеры ресурсов.
                    _application.Resources["BoolToColor"] =
                        new BoolToColorConverter();

                    _application.Resources["BoolToText"] =
                        new BoolToTextConverter();

                    // Получаем диспетчер созданного WPF-потока.
                    _dispatcher = Dispatcher.CurrentDispatcher;

                    // Сообщаем основному потоку, что WPF готов к работе.
                    started.Set();

                    // Запускаем цикл обработки UI-сообщений.
                    Dispatcher.Run();
                });

                // Устанавливаем STA для корректной работы WPF.
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.IsBackground = true;
                _thread.Start();

                // Ожидаем полной инициализации WPF-потока.
                started.Wait();
            }
        }
    }
    private class BoolToColorConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            // Возвращаем нейтральный цвет для тестового ресурса.
            return Brushes.Gray;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private class BoolToTextConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            // Преобразуем логическое значение в текст для тестового ресурса.
            return value is bool boolean && boolean ? "Да" : "Нет";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    #endregion
}