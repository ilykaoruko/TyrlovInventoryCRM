using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TyrlovInventoryCRM.Data;
using TyrlovInventoryCRM.Models;
using TyrlovInventoryCRM.Services;

namespace TyrlovInventoryCRM.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        [ObservableProperty]
        private string _newProductName = string.Empty;

        [ObservableProperty]
        private string _newProductCategory = string.Empty;

        [ObservableProperty]
        private decimal _newProductPrice;

        [ObservableProperty]
        private int _newProductQuantity;

        [ObservableProperty]
        private Product? _selectedProduct;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _selectedTab = 0;

        [ObservableProperty]
        private int _totalProductsCount;

        [ObservableProperty]
        private decimal _totalInventorySum;

        [ObservableProperty]
        private bool _isDarkTheme;

        // Данные для графика
        [ObservableProperty]
        private IEnumerable<ISeries> _categorySeries = Enumerable.Empty<ISeries>();

        // Данные для критического остатка (заканчивающиеся товары)
        [ObservableProperty]
        private ObservableCollection<Product> _lowStockProducts = new();

        public MainViewModel()
        {
            // 1. Применяем тему из файла настроек при старте
            var settings = SettingsService.LoadSettings();
            _isDarkTheme = settings.IsDarkTheme;
            ApplyThemeDictionary(_isDarkTheme);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                using var db = new AppDbContext();
                db.Database.EnsureCreated();
            });

            LoadProducts();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            using var db = new AppDbContext();
            var allProducts = db.Products.ToList();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(allProducts);
            }
            else
            {
                var filtered = allProducts.Where(p =>
                    p.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase));
                Products = new ObservableCollection<Product>(filtered);
            }
            UpdateDashboardStats();
        }

        [RelayCommand]
        private void LoadProducts()
        {
            using var db = new AppDbContext();
            var productsList = db.Products.ToList();
            Products = new ObservableCollection<Product>(productsList);
            UpdateDashboardStats();
        }

        private void UpdateDashboardStats()
        {
            TotalProductsCount = Products.Sum(p => p.Quantity);
            TotalInventorySum = Products.Sum(p => p.Price * p.Quantity);

            var categoryGroups = Products
                .GroupBy(p => p.Category)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key));

            var series = new List<ISeries>();
            foreach (var group in categoryGroups)
            {
                series.Add(new PieSeries<int>
                {
                    Values = new[] { group.Count() },
                    Name = group.Key,
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}"
                });
            }
            CategorySeries = series;

            var lowStock = Products
                .Where(p => p.Quantity < 5)
                .OrderBy(p => p.Quantity)
                .Take(5)
                .ToList();
            LowStockProducts = new ObservableCollection<Product>(lowStock);
        }

        [RelayCommand]
        private void SwitchToProducts() => SelectedTab = 0;

        [RelayCommand]
        private void SwitchToDashboard()
        {
            SelectedTab = 1;
            UpdateDashboardStats();
        }

        [RelayCommand]
        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(NewProductName)) return;

            var product = new Product
            {
                Name = NewProductName,
                Category = NewProductCategory,
                Price = NewProductPrice,
                Quantity = NewProductQuantity
            };

            using var db = new AppDbContext();
            db.Products.Add(product);
            db.SaveChanges();

            LoadProducts();

            NewProductName = string.Empty;
            NewProductCategory = string.Empty;
            NewProductPrice = 0;
            NewProductQuantity = 0;
        }

        [RelayCommand]
        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            using var db = new AppDbContext();
            db.Products.Remove(SelectedProduct);
            db.SaveChanges();

            LoadProducts();
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ApplyThemeDictionary(IsDarkTheme);

            // Сохраняем тему при переключении
            SettingsService.SaveSettings(IsDarkTheme);
        }

        private void ApplyThemeDictionary(bool isDark)
        {
            string themeName = isDark ? "DarkTheme" : "LightTheme";
            var uri = new System.Uri($"pack://application:,,,/Themes/{themeName}.xaml");

            var dict = new System.Windows.ResourceDictionary { Source = uri };
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        [RelayCommand]
        private void ExportExcel()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Отчет_Склад",
                DefaultExt = ".xlsx",
                Filter = "Excel Documents (.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                TyrlovInventoryCRM.Services.ReportService.ExportToExcel(Products, dialog.FileName);
                System.Windows.MessageBox.Show("Файл Excel успешно сохранен!", "Успех",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ExportPdf()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Отчет_Склад",
                DefaultExt = ".pdf",
                Filter = "PDF Documents (.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                TyrlovInventoryCRM.Services.ReportService.ExportToPdf(Products, dialog.FileName);
                System.Windows.MessageBox.Show("Файл PDF успешно сохранен!", "Успех",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
    }
}