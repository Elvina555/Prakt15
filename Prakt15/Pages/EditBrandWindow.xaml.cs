using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        private readonly Brand? _brand;
        private readonly bool _isNewBrand;

        // Конструктор для создания нового бренда
        public EditBrandWindow() : this(null) { }

        // Конструктор для редактирования существующего бренда
        public EditBrandWindow(Brand? brand)
        {
            InitializeComponent();
            _brand = brand ?? new Brand();
            _isNewBrand = (brand == null);

            if (_isNewBrand)
            {
                Title = "Добавление нового бренда";
            }

            Loaded += EditBrandWindow_Loaded;
        }

        private void EditBrandWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_brand != null && !string.IsNullOrEmpty(_brand.Name))
            {
                txtBrandName.Text = _brand.Name;
            }
            txtBrandName.SelectAll();
            txtBrandName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newName = txtBrandName.Text.Trim();
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Название бренда не может быть пустым", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtBrandName.Focus();
                    return;
                }

                bool exists = _db.Brands.Any(b =>
                    b.Name != null &&
                    b.Name.ToLower() == newName.ToLower() &&
                    (_brand.Id == 0 || b.Id != _brand.Id));

                if (exists)
                {
                    MessageBox.Show("Бренд с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtBrandName.SelectAll();
                    txtBrandName.Focus();
                    return;
                }
                _brand.Name = newName;
                if (_isNewBrand || _brand.Id == 0)
                {
                    _db.Brands.Add(_brand);
                }

                _db.SaveChanges();

                MessageBox.Show(_isNewBrand ? "Бренд успешно добавлен" : "Бренд успешно сохранен",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при сохранении бренда:\n\n{ex.Message}";

                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nДетали: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtBrandName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnSave_Click(sender, e);
            }
        }
    }
}
