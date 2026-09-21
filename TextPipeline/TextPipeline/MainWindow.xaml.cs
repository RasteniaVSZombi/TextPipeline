using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextPipeline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowContract_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel mainVm && mainVm.SelectedOperation != null)
            {
                var c = mainVm.SelectedOperation.Contract;

                // Собираем красивый текст из полей контракта
                string contractText =
                    $"{c.Title}\n\n" +
                    $"Предусловие:\n{c.Pre}\n\n" +
                    $"Постусловие:\n{c.Post}\n\n" +
                    $"Эффекты:\n{c.Effects}\n\n" +
                    $"Валидный пример:\n{c.ValidExample}\n\n" +
                    $"Невалидный пример:\n{c.InvalidExample}";

                var contractWindow = new ContractWindow
                {
                    Owner = this,
                    ContractContent = contractText
                };
                contractWindow.ShowDialog();
            }
        }

    }
}