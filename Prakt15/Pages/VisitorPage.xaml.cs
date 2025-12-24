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
    /// Логика взаимодействия для VisitorPage.xaml
    /// </summary>
    public partial class VisitorPage : Page
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private ObservableCollection<ProductDisplay> _products = new ObservableCollection<ProductDisplay>();
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        private ObservableCollection<Brand> _brands = new ObservableCollection<Brand>();
        private ICollectionView? _productsView;
        private string _searchQuery = "";
        private double? _priceFrom = null;
        private double? _priceTo = null;

        public VisitorPage()
        {
            InitializeComponent();
            Loaded += VisitorPage_Loaded;
        }

        private void VisitorPage_Loaded(object sender, RoutedEventArgs e)
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
                var categoryList = new List<Category> {
                    new Category { Id = 0, Name = "Все категории" }
                };
                categoryList.AddRange(categories);

                _categories.Clear();
                foreach (var category in categoryList)
                {
                    _categories.Add(category);
                }

                if (cmbCategory.ItemsSource == null)
                {
                    cmbCategory.ItemsSource = _categories;
                    cmbCategory.DisplayMemberPath = "Name";
                }
                if (cmbCategory.SelectedIndex == -1)
                {
                    cmbCategory.SelectedIndex = 0;
                }

                var brands = _db.Brands.ToList();
                var brandList = new List<Brand> {
                    new Brand { Id = 0, Name = "Все бренды" }
                };
                brandList.AddRange(brands);

                _brands.Clear();
                foreach (var brand in brandList)
                {
                    _brands.Add(brand);
                }

                if (cmbBrand.ItemsSource == null)
                {
                    cmbBrand.ItemsSource = _brands;
                    cmbBrand.DisplayMemberPath = "Name";
                }
                if (cmbBrand.SelectedIndex == -1)
                {
                    cmbBrand.SelectedIndex = 0;
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

            if (cmbCategory.SelectedItem is Category selectedCategory && selectedCategory.Id != 0)
            {
                if (product.CategoryName != selectedCategory.Name)
                    return false;
            }

            if (cmbBrand.SelectedItem is Brand selectedBrand && selectedBrand.Id != 0)
            {
                if (product.BrandName != selectedBrand.Name)
                    return false;
            }

            if (_priceFrom.HasValue && product.Price < _priceFrom.Value)
                return false;

            if (_priceTo.HasValue && product.Price > _priceTo.Value)
                return false;

            return true;
        }

        private void UpdateCounters()
        {
            txtTotalCount.Text = $"Всего товаров: {_products.Count}";
            int filteredCount = _productsView?.Cast<object>().Count() ?? 0;
            txtFilteredCount.Text = $"Показано: {filteredCount}";
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
            if (string.IsNullOrWhiteSpace(txtPriceFrom.Text))
                _priceFrom = null;
            else if (double.TryParse(txtPriceFrom.Text, out double fromPrice))
                _priceFrom = fromPrice;

            if (string.IsNullOrWhiteSpace(txtPriceTo.Text))
                _priceTo = null;
            else if (double.TryParse(txtPriceTo.Text, out double toPrice))
                _priceTo = toPrice;

            _productsView?.Refresh();
            UpdateCounters();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_productsView == null || cmbSort.SelectedItem == null) return;

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
                    _productsView.SortDescriptions.Add(new SortDescription(sortProperty, direction));
                }
            }

            _productsView.Refresh();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;
            cmbBrand.SelectedIndex = 0;
            txtPriceFrom.Clear();
            txtPriceTo.Clear();
            cmbSort.SelectedIndex = -1;

            _searchQuery = "";
            _priceFrom = null;
            _priceTo = null;
            _productsView?.SortDescriptions.Clear();
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
    }
}
