using Prakt15.Models;
using Prakt15.Services;
using Prakt15.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Логика взаимодействия для EditTagWindow.xaml
    /// </summary>
    public partial class EditTagWindow : Window
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private readonly Tag _tag;

        public EditTagWindow(Tag tag)
        {
            InitializeComponent();
            _tag = tag;
            Loaded += EditTagWindow_Loaded;
        }

        private void EditTagWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtTagName.Text = _tag.Name;
            txtTagName.SelectAll();
            txtTagName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newName = txtTagName.Text.Trim();

                if (!EntityValidator.ValidateName(newName, "тега", out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTagName.Focus();
                    return;
                }

                bool exists = _db.Tags.Any(t =>
                    t.Name.ToLower() == newName.ToLower() && t.Id != _tag.Id);

                if (exists)
                {
                    MessageBox.Show("Тег с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTagName.SelectAll();
                    txtTagName.Focus();
                    return;
                }

                _tag.Name = newName;
                _db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
