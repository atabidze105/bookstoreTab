using Avalonia.Controls;
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
            Filtration();
            ListBoxInit(_FoundedProducts.Count > 0 ? _FoundedProducts : _Products);
        }

        private void ListBoxInit(List<Product> products)
        {
            lbox_books.ItemsSource = products.Select(x => new
            {
                Header = $"{x.Name} ({x.AttachedProducts.Count})",
                ImageMain = System.IO.File.Exists($"Assets/{x.MainImagePath}") == true ? new Bitmap($"Assets/{x.MainImagePath}") : null,
                Price = $"{x.Cost} {System.Net.WebUtility.HtmlDecode("&#8381;")}" ,
                Background = x.IsActive == true ? "White" : "LightGray",
                Supplier = x.Manufacturer.Name
            });
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

        private void Filtration()
        {
            string? s = (cbox_filtration.SelectedItem!.ToString())!;
            if(s != "Все элементы")
                _FoundedProducts.AddRange(_Products.Where(x => x.Manufacturer.Name == s));
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            _FoundedProducts.Clear();
            Filtration();
            ListBoxInit(_FoundedProducts.Count > 0 ? _FoundedProducts : _Products);
        }
    }
}