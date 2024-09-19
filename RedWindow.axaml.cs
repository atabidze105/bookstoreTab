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
        _PictureFile = product != null ? product.MainImagePath : null; //���� ������ ����� �����������, �� ������ �� ���� �������� � ����, ����� null
        tblock_header.Text = "�������������� ������";
        btn_addItem.Content = "���������";
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
}