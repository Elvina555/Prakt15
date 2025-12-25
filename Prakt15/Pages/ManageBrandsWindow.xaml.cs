using Prakt15.Models;
using Prakt15.Services;
using Prakt15.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для ManageBrandsWindow.xaml
    /// </summary>
    public partial class ManageBrandsWindow : Window
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private ObservableCollection<Brand> _brands = new ObservableCollection<Brand>();

        public ManageBrandsWindow()
        {
            InitializeComponent();
            Loaded += ManageBrandsWindow_Loaded;
        }

        private void ManageBrandsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBrands();
        }

        private void LoadBrands()
        {
            try
            {
                _brands.Clear();
                var brands = _db.Brands
                    .OrderBy(b => b.Name)
                    .ToList();

                foreach (var brand in brands)
                {
                    _brands.Add(brand);
                }

                lstBrands.ItemsSource = _brands;
                txtNewBrand.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки брендов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string brandName = txtNewBrand.Text.Trim();

             
                if (string.IsNullOrWhiteSpace(brandName))
                {
                    MessageBox.Show("Название бренда не может быть пустым", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewBrand.Focus();
                    return;
                }

               
                bool exists = _db.Brands.Any(b =>
                    b.Name != null && b.Name.ToLower() == brandName.ToLower());

                if (exists)
                {
                    MessageBox.Show("Бренд с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewBrand.SelectAll();
                    txtNewBrand.Focus();
                    return;
                }
                int maxId = _db.Brands.Any() ? _db.Brands.Max(b => b.Id) : 0;
                int newId = maxId + 1;

                var newBrand = new Brand
                {
                    Id = newId,
                    Name = brandName
                };


                _db.Brands.Add(newBrand);
                _db.SaveChanges();

                MessageBox.Show($"Бренд \"{brandName}\" успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                txtNewBrand.Clear();
                LoadBrands();
                txtNewBrand.Focus();
            }
            catch (Exception ex)
            {

                StringBuilder errorMessage = new StringBuilder();
                errorMessage.AppendLine("Ошибка при добавлении бренда:");
                errorMessage.AppendLine(ex.Message);

                if (ex.InnerException != null)
                {
                    errorMessage.AppendLine();
                    errorMessage.AppendLine("Детали:");
                    errorMessage.AppendLine(ex.InnerException.Message);
                }

                MessageBox.Show(errorMessage.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null)
                {
                    if (int.TryParse(button.Tag.ToString(), out int brandId))
                    {
                        var brand = _db.Brands.FirstOrDefault(b => b.Id == brandId);

                        if (brand == null)
                        {
                            MessageBox.Show("Бренд не найден", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var editWindow = new EditBrandWindow(brand);
                        if (editWindow.ShowDialog() == true)
                        {
                            LoadBrands();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null)
                {
                    if (int.TryParse(button.Tag.ToString(), out int brandId))
                    {
                        var brand = _db.Brands.FirstOrDefault(b => b.Id == brandId);

                        if (brand == null)
                        {
                            MessageBox.Show("Бренд не найден", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        bool hasProducts = _db.Products.Any(p => p.BrandId == brandId);

                        if (hasProducts)
                        {
                            MessageBox.Show("Нельзя удалить бренд, к которому привязаны товары.\n" +
                                           "Сначала удалите или переместите все товары этого бренда.",
                                           "Ошибка удаления",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Error);
                            return;
                        }

                        var result = MessageBox.Show($"Вы действительно хотите удалить бренд \"{brand.Name}\"?\n" +
                                                    "Это действие нельзя отменить.",
                                                    "Подтверждение удаления",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _db.Brands.Remove(brand);
                                _db.SaveChanges();

                                MessageBox.Show($"Бренд \"{brand.Name}\" успешно удален", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadBrands();
                            }
                            catch (Exception deleteEx)
                            {
                                MessageBox.Show($"Ошибка при удалении: {deleteEx.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                lstBrands.ItemsSource = _brands;
                return;
            }

            string searchText = txtSearch.Text.ToLower();
            var filtered = _brands.Where(b =>
                b.Name != null && b.Name.ToLower().Contains(searchText))
                .ToList();

            lstBrands.ItemsSource = filtered;
        }

        private void TxtNewBrand_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnAdd_Click(sender, e);
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            lstBrands.ItemsSource = _brands;
        }
    }
}