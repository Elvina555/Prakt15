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
    /// Логика взаимодействия для ManageTagsWindow.xaml
    /// </summary>
    public partial class ManageTagsWindow : Window
    {
        private readonly YourDbContext _db = DBService.Instance.Context;
        private ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();

        public ManageTagsWindow()
        {
            InitializeComponent();
            Loaded += ManageTagsWindow_Loaded;
        }

        private void ManageTagsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        private void LoadTags()
        {
            try
            {
                _tags.Clear();

                var tags = _db.Tags
                    .OrderBy(t => t.Name)
                    .ToList();
                foreach (var tag in tags)
                {
                    _tags.Add(tag);
                }

                lstTags.ItemsSource = _tags;
                txtNewTag.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tagName = txtNewTag.Text.Trim();

           
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    MessageBox.Show("Название тега не может быть пустым", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewTag.Focus();
                    return;
                }

         
                bool exists = _db.Tags.Any(t =>
                    t.Name != null && t.Name.ToLower() == tagName.ToLower());

                if (exists)
                {
                    MessageBox.Show("Тег с таким названием уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNewTag.SelectAll();
                    txtNewTag.Focus();
                    return;
                }

                int maxId = _db.Tags.Any() ? _db.Tags.Max(t => t.Id) : 0;
                int newId = maxId + 1;

           
                var newTag = new Tag
                {
                    Id = newId, 
                    Name = tagName
                };

                _db.Tags.Add(newTag);
                _db.SaveChanges();

                MessageBox.Show($"Тег \"{tagName}\" успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                txtNewTag.Clear();
                LoadTags();
                txtNewTag.Focus();
            }
            catch (Exception ex)
            {
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.AppendLine("Ошибка при добавлении тега:");
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
                    if (int.TryParse(button.Tag.ToString(), out int tagId))
                    {
                        var tag = _db.Tags.FirstOrDefault(t => t.Id == tagId);

                        if (tag == null)
                        {
                            MessageBox.Show("Тег не найден", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var editWindow = new EditTagWindow(tag);
                        if (editWindow.ShowDialog() == true)
                        {
                            LoadTags();
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
                    if (int.TryParse(button.Tag.ToString(), out int tagId))
                    {
                        var tag = _db.Tags.FirstOrDefault(t => t.Id == tagId);

                        if (tag == null)
                        {
                            MessageBox.Show("Тег не найден", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        bool hasProducts = _db.ProductTags.Any(pt => pt.TagId == tagId);

                        if (hasProducts)
                        {
                            MessageBox.Show("Нельзя удалить тег, который используется в товарах.\n" +
                                           "Сначала удалите тег из всех товаров.",
                                           "Ошибка удаления",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Error);
                            return;
                        }

                        var result = MessageBox.Show($"Вы действительно хотите удалить тег \"{tag.Name}\"?\n" +
                                                    "Это действие нельзя отменить.",
                                                    "Подтверждение удаления",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _db.Tags.Remove(tag);
                                _db.SaveChanges();

                                MessageBox.Show($"Тег \"{tag.Name}\" успешно удален", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                LoadTags();
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
                lstTags.ItemsSource = _tags;
                return;
            }

            string searchText = txtSearch.Text.ToLower();
            var filtered = _tags.Where(t =>
                t.Name != null && t.Name.ToLower().Contains(searchText))
                .ToList();

            lstTags.ItemsSource = filtered;
        }

        private void TxtNewTag_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnAdd_Click(sender, e);
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            lstTags.ItemsSource = _tags;
        }
    }
}

