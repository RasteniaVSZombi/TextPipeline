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
using Xunit;

namespace Tests;

public class UITests
{
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
}