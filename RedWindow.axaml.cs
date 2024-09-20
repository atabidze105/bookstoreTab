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
    private string? _PictureFile = null; //�����������, ������� ���������� ����� ������
    private string? _SelectedImage = null; //��������� �����������

    private readonly FileDialogFilter fileFilter = new() //������ ��� ����������
    {
        Extensions = new List<string>() { "jpg", "png" }, //��������� ����������, ������������ � ����������
        Name = "����� �����������" //���������
    };

    public RedWindow()
    {
        InitializeComponent();
        CboxSuppliersInit();
        tbox_id.IsVisible = false;
        tblock_header.Text = "�������� ������";
        btn_addItem.Content = "��������";
    }

    public RedWindow(Product product)
    {
        InitializeComponent();
        CboxSuppliersInit();
        _RedProduct = product;
        _PictureFile = _RedProduct != null ? _RedProduct.MainImagePath : null; //���� ������ ����� �����������, �� ������ �� ���� �������� � ����, ����� null
        tblock_header.Text = "�������������� ������";
        btn_addItem.Content = "���������";
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

    private async void ImageSelection(object? sender, Avalonia.Interactivity.RoutedEventArgs e) //����� � �������� �����������
    {
        var btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_addImage": //����������
                OpenFileDialog dialog = new(); //�������� ����������
                dialog.Filters.Add(fileFilter); //���������� �������
                string[] result = await dialog.ShowAsync(this); //����� �����
                if (result == null || result.Length == 0 || new System.IO.FileInfo(result[0]).Length > 2000000)
                    return;//���� ������� ��������� ��� ������ ����� ����� ��������� 2 ��, �� �������� �� ����� �������

                string imageName = System.IO.Path.GetFileName(result[0]); //��������� ����� �����
                string[] extention = imageName.Split('.'); //�������� ����� ������� �� �������� � ����������
                string temp = extention[0]; //� ���������� ���������� �������� �������� �����. ��� ����� �������� � ��������
                int i = 1; //�������
                while (SameName(temp) != null) //���� ����� ��� �������� ������������ ����� ���������� �������� �����
                {
                    temp = extention[0] + $"{i}"; //����� ��� �����
                    i++;
                }
                imageName = temp + '.' + extention[1]; //����� ��� ����� � �����������

                System.IO.File.Copy(result[0], $"Assets/{imageName}", true); //����������� ����� � ����� �������

                tblock_productPhoto.Text = imageName;
                image_productPhoto.Source = new Bitmap($"Assets/{imageName}");
                tblock_productPhoto.IsVisible = image_productPhoto.IsVisible = btn_deleteImage.IsVisible = true;

                if (_SelectedImage != null && _SelectedImage != _PictureFile) //���� �� ��������� ����� �������� ���� ������� ������, � ��� ���� ��������� �������� �� �������� �� ����, �������� ������������ ����������� ������
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //�������� ����������� ����������� �� �������
                _SelectedImage = imageName;

                break;
            case "btn_deleteImage": //��������
                tblock_productPhoto.IsVisible = image_productPhoto.IsVisible = btn_deleteImage.IsVisible = false;

                if (_SelectedImage != _PictureFile) //�������� ���������� ������ ���� ��������� ����������� �� �������� ��������� �� ����, �������� ������������ ����������� �������
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //�������� ���������� �����������

                _SelectedImage = null;//������� ���� � ��������� ������������
                break;
        }
    }

    private string SameName(string filename) //�������� ������������ ����� �����
    {
        string[] withExtentions = Directory.GetFiles("Assets"); //��������� �������� ���� ����������� �� ������� � ������������
        List<string> withoutExtentions = []; //������������� ������ ������ ��� �������� ������ ��� ����������

        foreach (string file in withExtentions)
            withoutExtentions.Add(System.IO.Path.GetFileNameWithoutExtension(file)); //� ����� ������ ���������� �������� ������ ��� ����������

        foreach (string file1 in withoutExtentions) //������� ������� �������� ����� �� ������ ��������
            if (file1 == filename) //���� �������� ������ �� ������ ��������� �������� ����� ��������� � ������
            {
                return filename; //���������� �������� �����
            }
        return null; //���� ����� ���� �� ��� ������, ���������� null
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