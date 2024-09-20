using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using bookshopTab.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bookshopTab;

public partial class RedWindow : Window
{
    private Product _RedProduct = null;
    //private List<>
    private string? _PictureFile = null; //изображение, которое изначально имеет объект
    private string? _SelectedImage = null; //выбранное изображение

    private readonly FileDialogFilter fileFilter = new() //Фильтр для проводника
    {
        Extensions = new List<string>() { "jpg", "png" }, //доступные расширения, отображаемые в проводнике
        Name = "Файлы изображений" //пояснение
    };

    public RedWindow()
    {
        InitializeComponent();
        CboxSuppliersInit();
        tbox_id.IsVisible = false;
        tblock_header.Text = "Создание товара";
        btn_addItem.Content = "Добавить";
    }

    public RedWindow(Product product)
    {
        InitializeComponent();
        CboxSuppliersInit();
        _RedProduct = product;
        _PictureFile = _RedProduct != null ? _RedProduct.MainImagePath : null; //если объект имеет изображение, то ссылка на него хранится в поле, иначе null
        tblock_header.Text = "Редактирование товара";
        btn_addItem.Content = "Сохранить";
        btn_delete.IsVisible = true;
        DataDisplaying(product);
    }

    private void DataDisplaying(Product product)
    {
        tbox_id.Text = product.ProductId.ToString();
        tbox_name.Text = product.Name;
        tbox_cost.Text = string.Format("{0:0.00}",product.Cost);
        tbox_description.Text = product.Description;
        chbox_isActive.IsChecked = product.IsActive;
        cbox_suppliers.SelectedItem = product.Manufacturer.Name;
        if (File.Exists($"Assets/{product.MainImagePath}"))
        {
            tblock_productPhoto.Text = _SelectedImage = product.MainImagePath;
            image_productPhoto.Source = new Bitmap($"Assets/{product.MainImagePath}");
            tblock_productPhoto.IsVisible = image_productPhoto.IsVisible = btn_deleteImage.IsVisible = true;
        }
        lbox_attachedProducts.ItemsSource = product.AttachedProducts.ToList();
    }

    private void NewDataApplying(Product product)
    {
        product.Name = tbox_name.Text;
        product.Description = tbox_description.Text;
        product.Cost = Convert.ToDouble(tbox_cost.Text);
        product.IsActive = (bool)chbox_isActive.IsChecked!;
        if (cbox_suppliers.SelectedIndex != 0)
            product.ManufacturerId = Helper.Database.Manufacturers.ToList().FindIndex(x => x.Name == cbox_suppliers.SelectedItem) + 1;
        else
            product.ManufacturerId = null;
        product.MainImagePath = _SelectedImage;
    }

    private void ShowMainWindow()
    {

        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }

    private void FormActivity(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Button btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_addItem":
                if (tbox_name.Text != "" && tbox_cost.Text != "" && cbox_suppliers.SelectedIndex != 0)
                {

                    if (_RedProduct == null)
                    {
                        Product product = new();
                        product.ProductId = Helper.Database.Products.OrderByDescending(x => x.ProductId).ToList().First().ProductId + 1;
                        NewDataApplying(product);
                        Helper.Database.Products.Add(product);
                        Helper.Database.SaveChanges();
                    }
                    else
                    {
                        NewDataApplying(_RedProduct);
                        Helper.Database.SaveChanges();
                        if (_SelectedImage != _PictureFile && _PictureFile != null)
                            System.IO.File.Delete($"Assets/{_PictureFile}");
                    }
                    ShowMainWindow();
                }
                break;
            case "btn_delete":
                if (_SelectedImage != null) 
                    System.IO.File.Delete($"Assets/{_SelectedImage}");
                if (_PictureFile != null)
                    System.IO.File.Delete($"Assets/{_PictureFile}");
                Helper.Database.Products.Remove(_RedProduct);
                Helper.Database.SaveChanges();
                ShowMainWindow();
                break;
            case "btn_return":
                if (_SelectedImage != null && _SelectedImage != _PictureFile) 
                    System.IO.File.Delete($"Assets/{_SelectedImage}");
                ShowMainWindow();
                break;
        }
    }

    private void CboxSuppliersInit()
    {
        List<Manufacturer> manufacturers = [];
        manufacturers.AddRange(Helper.Database.Manufacturers.ToList());
        foreach (Manufacturer manufacturer in manufacturers)
        {
            cbox_suppliers.Items.Add(manufacturer.Name);
        }

    }

    private void TextBox_ManufacturerSearch(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        TextBox tboxItem = cbox_suppliers.Items[0] as TextBox;
        cbox_suppliers.Items.Clear();
        cbox_suppliers.Items.Add(tboxItem);
        if (tboxItem.Text != "")
            foreach (Manufacturer manufacturer in Helper.Database.Manufacturers.ToList())
            {
                if (manufacturer.Name.Trim().ToLower().Contains(tboxItem.Text.Trim().ToLower()))
                    cbox_suppliers.Items.Add(manufacturer.Name);
                                    
            }
        else
        {
            CboxSuppliersInit();
        }
    }

    private async void ImageSelection(object? sender, Avalonia.Interactivity.RoutedEventArgs e) //Выбор и удаление изображения
    {
        var btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_addImage": //добавление
                OpenFileDialog dialog = new(); //Открытие проводника
                dialog.Filters.Add(fileFilter); //Применение фильтра
                string[] result = await dialog.ShowAsync(this); //Выбор файла
                if (result == null || result.Length == 0 || new System.IO.FileInfo(result[0]).Length > 2000000)
                    return;//Если закрыть проводник или размер файла будет превышать 2 МБ, то картинка не будет выбрана

                string imageName = System.IO.Path.GetFileName(result[0]); //получение имени файла
                string[] extention = imageName.Split('.'); //Название файла делится на название и расширение
                string temp = extention[0]; //В изменяемой переменной хранится название файла. Оно будет меняться в процессе
                int i = 1; //Счетчик
                while (SameName(temp) != null) //Пока метод для проверки уникальности файла возвращает название файла
                {
                    temp = extention[0] + $"{i}"; //Новое имя файла
                    i++;
                }
                imageName = temp + '.' + extention[1]; //Новое имя файла с расширением

                System.IO.File.Copy(result[0], $"Assets/{imageName}", true); //Копирование файла в папку ассетов

                tblock_productPhoto.Text = imageName;
                image_productPhoto.Source = new Bitmap($"Assets/{imageName}");
                tblock_productPhoto.IsVisible = image_productPhoto.IsVisible = btn_deleteImage.IsVisible = true;

                if (_SelectedImage != null && _SelectedImage != _PictureFile) //Если до установки новой картинки была выбрана другая, и при этом выбранная картинка не значение из поля, хранящее изначальноне изображение товара
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //Удаление предыдущего изображения из ассетов
                _SelectedImage = imageName;

                break;
            case "btn_deleteImage": //удаление
                tblock_productPhoto.IsVisible = image_productPhoto.IsVisible = btn_deleteImage.IsVisible = false;

                if (_SelectedImage != _PictureFile) //Удаление произойдет только если удаляемое изображение не является значением из поля, хранящее изначальноне изображение объекта
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //Удаление выбранного изображения

                _SelectedImage = null;//очистка поля с выбранным изображением
                break;
        }
    }

    private string SameName(string filename) //Проверка уникальности имени файла
    {
        string[] withExtentions = Directory.GetFiles("Assets"); //Получение названий всех изображений из ассетов с расширениями
        List<string> withoutExtentions = []; //инициализация нового списка для названий файлов без расширений

        foreach (string file in withExtentions)
            withoutExtentions.Add(System.IO.Path.GetFileNameWithoutExtension(file)); //В новый список передаются названия файлов без расширений

        foreach (string file1 in withoutExtentions) //перебор каждого названия файла из списка названий
            if (file1 == filename) //если название одного из файлов идентично названию файла заданного в методе
            {
                return filename; //возвращает название файла
            }
        return null; //если такой файл не был найден, возвращает null
    }

    private void StackPanel_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var product = lbox_attachedProducts.SelectedItem as Product;
        RedWindow redWindow = new RedWindow(product);
        redWindow.Show();
        Close();
    }

    private void ButtonAdd_Click_ShowFlyout(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Button btn = (sender as Button)!;
        btn.Flyout.ShowAt(tbox_name);
        lbox_addAttachedProduct.ItemsSource = Helper.Database.Products.Where(x => x.ProductId.ToString() != tbox_id.Text && x.IsActive == true).ToList();
    }

    private void StackPanel_AddToAttachedProducts(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var item = sender as ListBoxItem;

    }
}