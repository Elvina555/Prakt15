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
                var brands = _db.Brands.ToList();
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

                if (!EntityValidator.ValidateName(brandName, "бренда", out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
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

                var newBrand = new Brand
                {
                    Name = brandName
                };

                _db.Brands.Add(newBrand);
                _db.SaveChanges();

                MessageBox.Show($"Бренд \"{brandName}\" успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                txtNewBrand.Clear();
                LoadBrands();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении бренда: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag != null)
                {
                    if (double.TryParse(button.Tag.ToString(), out double brandId))
                    {
                        var brand = _db.Brands
                            .FirstOrDefault(b => Math.Abs(b.Id - brandId) < 0.001);

                        if (brand == null) return;

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
                    if (double.TryParse(button.Tag.ToString(), out double brandId))
                    {
                        var brand = _db.Brands
                            .FirstOrDefault(b => Math.Abs(b.Id - brandId) < 0.001);

                        if (brand == null) return;

                        bool hasProducts = _db.Products.Any(p =>
                            p.BrandId.HasValue && Math.Abs(p.BrandId.Value - brandId) < 0.001);

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
                            _db.Brands.Remove(brand);
                            _db.SaveChanges();

                            MessageBox.Show($"Бренд \"{brand.Name}\" успешно удален", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadBrands();
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
                b.Name != null && b.Name.ToLower().Contains(searchText)).ToList();

            lstBrands.ItemsSource = filtered;
        }
    }
}
