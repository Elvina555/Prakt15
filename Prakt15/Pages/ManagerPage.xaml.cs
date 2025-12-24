using Microsoft.EntityFrameworkCore;
using Prakt15.Models;
using Prakt15.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private ObservableCollection<ProductDisplay> _products = new ObservableCollection<ProductDisplay>();
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private ObservableCollection<Brand> _brands = new ObservableCollection<Brand>();
        private ICollectionView? _productsView;
        private string _searchQuery = "";
        private string _priceFrom = "";
        private string _priceTo = "";

        public ManagerPage()
        {
            InitializeComponent();
            Loaded += ManagerPage_Loaded;
        }

        private void ManagerPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            SetupCollectionView();
        }

        private void LoadData()
        {
            try
            {
                _products.Clear();
                _categories.Clear();
                _brands.Clear();

                var products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .ToList();

                foreach (var product in products)
                {
                    _products.Add(new ProductDisplay(product));
                }

                var categories = _db.Categories.ToList();
                foreach (var category in categories)
                {
                    _categories.Add(category);
                }

                if (cmbCategory.ItemsSource == null)
                {
                    cmbCategory.ItemsSource = _categories;
                    cmbCategory.DisplayMemberPath = "Name";
                }

                var brands = _db.Brands.ToList();
                foreach (var brand in brands)
                {
                    _brands.Add(brand);
                }

                if (cmbBrand.ItemsSource == null)
                {
                    cmbBrand.ItemsSource = _brands;
                    cmbBrand.DisplayMemberPath = "Name";
                }

                UpdateCounters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupCollectionView()
        {
            _productsView = CollectionViewSource.GetDefaultView(_products);
            _productsView.Filter = FilterProduct;
            listViewProducts.ItemsSource = _productsView;
        }

        private bool FilterProduct(object obj)
        {
            if (obj is not ProductDisplay product)
                return false;

            if (!string.IsNullOrEmpty(_searchQuery))
            {
                string searchLower = _searchQuery.ToLower();
                bool nameMatch = product.Name.ToLower().Contains(searchLower);
                bool descMatch = product.Description?.ToLower().Contains(searchLower) ?? false;
                if (!nameMatch && !descMatch)
                    return false;
            }

            if (cmbCategory.SelectedItem is Category selectedCategory &&
                product.CategoryName != selectedCategory.Name)
                return false;

            if (cmbBrand.SelectedItem is Brand selectedBrand &&
                product.BrandName != selectedBrand.Name)
                return false;

            if (!string.IsNullOrEmpty(_priceFrom) &&
                double.TryParse(_priceFrom, out double minPrice) &&
                product.Price < minPrice)
                return false;

            if (!string.IsNullOrEmpty(_priceTo) &&
                double.TryParse(_priceTo, out double maxPrice) &&
                product.Price > maxPrice)
                return false;

            return true;
        }

        private void UpdateCounters()
        {
            txtTotalCount.Text = $"Всего товаров: {_products.Count}";
            txtFilteredCount.Text = $"Показано: {(_productsView?.Cast<object>().Count() ?? 0)}";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearch.Text;
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void CmbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void TxtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    if (!double.TryParse(textBox.Text, out double price))
                    {
                        textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                        textBox.ToolTip = "Введите корректную цену (только цифры и точка)";
                        return;
                    }

                    if (price < 0)
                    {
                        textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                        textBox.ToolTip = "Цена не может быть отрицательной";
                        return;
                    }
                }
                textBox.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 0, 196, 180));
                textBox.ToolTip = null;
            }

            _priceFrom = txtPriceFrom.Text;
            _priceTo = txtPriceTo.Text;
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void TxtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            if (sender is TextBox textBox && e.Text == "." && textBox.Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_productsView == null) return;

            _productsView.SortDescriptions.Clear();

            if (cmbSort.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                string sortProperty = "";
                var direction = ListSortDirection.Ascending;

                switch (selectedItem.Tag.ToString())
                {
                    case "Name":
                        sortProperty = "Name";
                        break;
                    case "PriceAsc":
                        sortProperty = "Price";
                        break;
                    case "PriceDesc":
                        sortProperty = "Price";
                        direction = ListSortDirection.Descending;
                        break;
                    case "QuantityAsc":
                        sortProperty = "Stock";
                        break;
                    case "QuantityDesc":
                        sortProperty = "Stock";
                        direction = ListSortDirection.Descending;
                        break;
                }

                if (!string.IsNullOrEmpty(sortProperty))
                {
                    _productsView.SortDescriptions.Add(
                        new SortDescription(sortProperty, direction));
                }
            }

            _productsView.Refresh();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = -1;
            cmbBrand.SelectedIndex = -1;
            txtPriceFrom.Clear();
            txtPriceTo.Clear();
            cmbSort.SelectedIndex = -1;

            _searchQuery = "";
            _priceFrom = "";
            _priceTo = "";
            _productsView?.SortDescriptions.Clear();
            _productsView?.Refresh();
            UpdateCounters();
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditProductWindow();
            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void BtnManageCategories_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageCategoriesWindow();
            window.ShowDialog();
            LoadData();
        }

        private void BtnManageBrands_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageBrandsWindow();
            window.ShowDialog();
            LoadData();
        }

        private void BtnManageTags_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageTagsWindow();
            window.ShowDialog();
            LoadData();
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                double.TryParse(button.Tag.ToString(), out double productId))
            {
                try
                {
                    var product = _db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductTags)
                            .ThenInclude(pt => pt.Tag)
                        .FirstOrDefault(p => p.Id == productId);

                    if (product != null)
                    {
                        var window = new EditProductWindow(product);
                        if (window.ShowDialog() == true)
                        {
                            LoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке товара для редактирования: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null &&
                double.TryParse(button.Tag.ToString(), out double productId))
            {
                var product = _db.Products.Find(productId);

                if (product == null) return;

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить товар \"{product.Name}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var productTags = _db.ProductTags.Where(pt => pt.ProductId == productId).ToList();
                        _db.ProductTags.RemoveRange(productTags);

                        _db.Products.Remove(product);
                        _db.SaveChanges();

                        MessageBox.Show("Товар успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ListViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Пустая реализация
        }

        private void TxtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _productsView?.Refresh();
                UpdateCounters();
            }
        }

        private void TxtPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _productsView?.Refresh();
                UpdateCounters();
            }
        }
    }
}
