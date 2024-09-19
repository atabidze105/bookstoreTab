using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using bookshopTab.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bookshopTab;

public partial class RedWindow : Window
{
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
        _PictureFile = product != null ? product.MainImagePath : null; //если объект имеет изображение, то ссылка на него хранится в поле, иначе null
        tblock_header.Text = "Редактирование товара";
        btn_addItem.Content = "Сохранить";
        DataDisplaying(product);
    }

    private void DataDisplaying(Product product)
    {
        tbox_id.Text = product.ProductId.ToString();
        tbox_name.Text = product.Name;
        tbox_cost.Text = product.Cost.ToString();
        tbox_description.Text = product.Description;
        if (File.Exists(product.MainImagePath))
        {
            tblock_productPhoto.Text = product.MainImagePath;
            image_productPhoto.Source = new Bitmap($"Assets/{product.MainImagePath}");
            tblock_productPhoto.IsVisible = image_productPhoto.IsVisible = btn_deleteImage.IsVisible = true;
        }
    }

    private void FormActivity(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
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
}