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
    /// Логика взаимодействия для EditProductWindow.xaml
    /// </summary>
    public partial class EditProductWindow : Window
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private Product? _product;
        private List<TagViewModel> _tagViewModels = new List<TagViewModel>();

        public EditProductWindow(Product? product = null)
        {
            InitializeComponent();
            _product = product;
            LoadData();
            if (product != null)
                LoadProductData();
        }

        private void LoadData()
        {
            try
            {
                var categories = _db.Categories.ToList();
                cmbCategory.ItemsSource = categories;
                cmbCategory.DisplayMemberPath = "Name";

                var brands = _db.Brands.ToList();
                cmbBrand.ItemsSource = brands;
                cmbBrand.DisplayMemberPath = "Name";

                var allTags = _db.Tags.ToList();
                _tagViewModels = allTags.Select(t => new TagViewModel
                {
                    Id = t.Id,
                    Name = t.Name ?? string.Empty,
                    IsSelected = false
                }).ToList();

                lstTags.ItemsSource = _tagViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData()
        {
            if (_product == null) return;

            try
            {
                txtName.Text = _product.Name ?? string.Empty;
                txtDescription.Text = _product.Description ?? string.Empty;
                txtPrice.Text = _product.Price?.ToString() ?? "0";
                txtStock.Text = _product.Stock?.ToString() ?? "0";
                txtRating.Text = _product.Rating?.ToString() ?? "0";
                txtCreatedAt.Text = _product.CreatedAt ?? string.Empty;

                if (_product.CategoryId.HasValue)
                {
                    var category = _db.Categories
                        .FirstOrDefault(c => c.Id == _product.CategoryId.Value);

                    if (category != null)
                    {
                        cmbCategory.SelectedItem = category;
                    }
                }

                if (_product.BrandId.HasValue)
                {
                    var brand = _db.Brands
                        .FirstOrDefault(b => b.Id == _product.BrandId.Value);

                    if (brand != null)
                    {
                        cmbBrand.SelectedItem = brand;
                    }
                }

                if (_product.ProductTags != null && _product.ProductTags.Count > 0)
                {
                    var selectedTagIds = _product.ProductTags
                        .Where(pt => pt.TagId.HasValue)
                        .Select(pt => pt.TagId!.Value)
                        .ToList();

                    foreach (var tagVm in _tagViewModels)
                    {
                        tagVm.IsSelected = selectedTagIds.Contains(tagVm.Id);
                    }

                    lstTags.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ProductValidator.ValidateRequiredField(txtName.Text, "название товара", out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtName.Focus();
                    return;
                }

                if (!ProductValidator.ValidatePrice(txtPrice.Text, out double price, out errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPrice.Focus();
                    return;
                }

                if (!ProductValidator.ValidateStock(txtStock.Text, out double stock, out errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtStock.Focus();
                    return;
                }

                if (!ProductValidator.ValidateRequiredField(txtCreatedAt.Text, "дату создания", out errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtCreatedAt.Focus();
                    return;
                }

                if (!ProductValidator.ValidateRating(txtRating.Text, out double? rating, out errorMessage))
                {
                    MessageBox.Show(errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtRating.Focus();
                    return;
                }

                if (cmbCategory.SelectedItem == null)
                {
                    MessageBox.Show("Выберите категорию", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbCategory.Focus();
                    return;
                }

                if (cmbBrand.SelectedItem == null)
                {
                    MessageBox.Show("Выберите бренд", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    cmbBrand.Focus();
                    return;
                }

                _product ??= new Product();

                _product.Name = txtName.Text;
                _product.Description = txtDescription.Text;
                _product.Price = price;
                _product.Stock = stock;
                _product.CreatedAt = txtCreatedAt.Text;
                _product.Rating = rating;

                _product.CategoryId = ((Category)cmbCategory.SelectedItem).Id;
                _product.BrandId = ((Brand)cmbBrand.SelectedItem).Id;

                if (_product.Id == 0)
                {
                    _db.Products.Add(_product);
                }
                else
                {
                    _db.Products.Update(_product);
                }

                _db.SaveChanges();

                var selectedTagIds = _tagViewModels.Where(t => t.IsSelected).Select(t => t.Id).ToList();
                var currentProductTags = _db.ProductTags.Where(pt => pt.ProductId == _product.Id).ToList();

                var tagsToRemove = currentProductTags
                    .Where(pt => pt.TagId.HasValue && !selectedTagIds.Contains(pt.TagId.Value))
                    .ToList();

                _db.ProductTags.RemoveRange(tagsToRemove);

                var currentTagIds = currentProductTags
                    .Where(pt => pt.TagId.HasValue)
                    .Select(pt => pt.TagId!.Value)
                    .ToList();

                var tagsToAdd = selectedTagIds
                    .Where(tagId => !currentTagIds.Contains(tagId))
                    .Select(tagId => new ProductTag
                    {
                        ProductId = _product.Id,
                        TagId = tagId
                    })
                    .ToList();

                _db.ProductTags.AddRange(tagsToAdd);
                _db.SaveChanges();

                MessageBox.Show("Товар успешно сохранен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void TxtStock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void TxtRating_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

