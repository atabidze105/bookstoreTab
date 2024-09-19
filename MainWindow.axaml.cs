using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using bookshopTab.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;
using static bookshopTab.StaticData;

namespace bookshopTab
{
    public partial class MainWindow : Window
    {
        private List<Product> _Products = Helper.Database.Products.
            Include(x => x.Productphotos).
            Include(x => x.Productsales).
            Include(x => x.MainProducts).
            Include(x => x.AttachedProducts).
            Include(x => x.Manufacturer).ToList();
        private List<Product> _FoundedProducts = [];
        public MainWindow()
        {
            InitializeComponent();
            CboxFiltrationInit();
            
            tbox_searchbar.Text = _SearchbarContent;
            cbox_sorting.SelectedIndex = _SortingItemIndex;
            cbox_filtration.SelectedIndex = _FiltrationItemIndex;
            FiltrationAndSorting();
        }

        private void ListBoxInit(List<Product> products)
        {
            lbox_books.ItemsSource = products.ToList();
        }

        private void FiltrationAndSorting()
        {
            _FoundedProducts.Clear();
            Filtration();
            ListBoxInit(Searching());
            ItemsCountUpdate();
        }

        private void CboxFiltrationInit()
        {
            List<Manufacturer> manufacturers = [];
            manufacturers.AddRange(Helper.Database.Manufacturers.ToList());
            foreach (Manufacturer manufacturer in manufacturers)
            {
                cbox_filtration.Items.Add(manufacturer.Name);
            }
        }

        private void ItemsCountUpdate()
        {
            tblock_itemsCount.Text = $"{(_FoundedProducts.Count > 0 || cbox_filtration.SelectedIndex != 0 ? _FoundedProducts.Count : _Products.Count)} / {_Products.Count}";
        }

        private void Filtration()
        {
            
            if (cbox_filtration.SelectedItem != null)
            {
                string? s = (cbox_filtration.SelectedItem!.ToString())!;
                if (s != "Все элементы")
                    _FoundedProducts.AddRange(_Products.Where(x => x.Manufacturer.Name == s));
            }
        }

        private List<Product> BubbleSorting(List<Product> products ,int selectedOption) //Сортировка пузырьком
        {
            List<Product> bubble = []; //Список для сортировки пузырьком
            bubble.AddRange(products);
            switch (selectedOption)
            {
                case 0:
                    return products;
                case 1: //По убыванию
                    {
                        for (int i = 0; i < bubble.Count; i++)
                            for (int j = 0; j < bubble.Count - i - 1; j++)
                            {
                                if (bubble[j].Cost < bubble[j + 1].Cost)
                                {
                                    Product temp = bubble[j];
                                    bubble[j] = bubble[j + 1];
                                    bubble[j + 1] = temp;
                                }
                            }
                    }            
                    break;
                case 2: //По возрастанию
                    {
                        for (int i = 0; i < bubble.Count; i++)
                            for (int j = 0; j < bubble.Count - i - 1; j++)
                            {
                                if (bubble[j].Cost > bubble[j + 1].Cost)
                                {
                                    Product temp = bubble[j];
                                    bubble[j] = bubble[j + 1];
                                    bubble[j + 1] = temp;
                                }
                            }
                    }
                    break;
            }
            products.Clear();
            products.AddRange(bubble);
            return products;
        }

        private List<Product> Searching()
        {
            if (tbox_searchbar.Text != "")
            {
                List<Product> unsortedProducts = [];
                unsortedProducts.AddRange(_FoundedProducts.Count > 0 || cbox_filtration.SelectedIndex != 0 ? _FoundedProducts : _Products);
                int prioriryLevel;
                List<Product> productsPriorityLevel1 = [];
                List<Product> productsPriorityLevel2 = [];
                foreach (Product product in unsortedProducts)
                {
                    prioriryLevel = 0;
                    if (product.Name.Trim().ToLower().Contains(tbox_searchbar.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    if (product.Description.Trim().ToLower().Contains(tbox_searchbar.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    switch (prioriryLevel)
                    {
                        case 1:
                            productsPriorityLevel1.Add(product);
                            break;
                        case 2:
                            productsPriorityLevel2.Add(product);
                            break;
                    }
                }
                _FoundedProducts.Clear();
                _FoundedProducts.AddRange(BubbleSorting(productsPriorityLevel2, cbox_sorting.SelectedIndex));
                _FoundedProducts.AddRange(BubbleSorting(productsPriorityLevel1, cbox_sorting.SelectedIndex));
                return _FoundedProducts;
            }
            else
            {
                return BubbleSorting(_FoundedProducts.Count > 0 || cbox_filtration.SelectedIndex != 0 ? _FoundedProducts : _Products, cbox_sorting.SelectedIndex);
            }
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            FiltrationAndSorting();
        }

        private void tbox_searchActivity(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            FiltrationAndSorting();
        }

        private void StackPanel_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            _SearchbarContent = tbox_searchbar.Text;
            _FiltrationItemIndex = cbox_filtration.SelectedIndex;
            _SortingItemIndex = cbox_sorting.SelectedIndex;
            var product = lbox_books.SelectedItem as Product;
            RedWindow redWindow = new RedWindow(product);
            redWindow.Show();
            Close();
        }

        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _SearchbarContent = tbox_searchbar.Text;
            _FiltrationItemIndex = cbox_filtration.SelectedIndex;
            _SortingItemIndex = cbox_sorting.SelectedIndex;
            RedWindow redWindow = new RedWindow();
            redWindow.Show();
            Close();
        }
    }
}