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
    /// Логика взаимодействия для EditBrandWindow.xaml
    /// </summary>
    public partial class EditBrandWindow : Window
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private readonly Brand _brand;

        public EditBrandWindow(Brand brand)
        {
            InitializeComponent();
            _brand = brand;
            Loaded += EditBrandWindow_Loaded;
        }

        private void EditBrandWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtBrandName.Text = _brand.Name;
            txtBrandName.SelectAll();
            txtBrandName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newName = txtBrandName.Text.Trim();

                if (!EntityValidator.ValidateName(newName, "бренда", out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtBrandName.Focus();
                    return;
                }

                bool exists = _db.Brands.Any(b =>
                    b.Name.ToLower() == newName.ToLower() && b.Id != _brand.Id);

                if (exists)
                {
                    MessageBox.Show("Бренд с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtBrandName.SelectAll();
                    txtBrandName.Focus();
                    return;
                }

                _brand.Name = newName;
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
