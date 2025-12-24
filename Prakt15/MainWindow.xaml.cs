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

namespace Prakt15
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

        private void BtnVisitor_Click(object sender, RoutedEventArgs e)
        {
            var visitorPage = new VisitorPage();
            mainFrame.Navigate(visitorPage);
        }

        private void BtnManager_Click(object sender, RoutedEventArgs e)
        {
            if (txtPinCode.Text == "1234")
            {
                var managerPage = new ManagerPage();
                mainFrame.Navigate(managerPage);
            }
            else
            {
                MessageBox.Show("Неверный пин-код! Для входа как менеджер введите: 1234",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPinCode.Clear();
                txtPinCode.Focus();
            }
        }
    }
}