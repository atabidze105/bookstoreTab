using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace bookshopTab.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public double Cost { get; set; }

    public string? Description { get; set; }

    public string? MainImagePath { get; set; }

    public bool IsActive { get; set; }

    public int? ManufacturerId { get; set; }

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual ICollection<Productphoto> Productphotos { get; set; } = new List<Productphoto>();

    public virtual ICollection<Productsale> Productsales { get; set; } = new List<Productsale>();

    public virtual ICollection<Product> AttachedProducts { get; set; } = new List<Product>();

    public virtual ICollection<Product> MainProducts { get; set; } = new List<Product>();

    public Bitmap? ImageMain => System.IO.File.Exists($"Assets/{MainImagePath}") == true ? new Bitmap($"Assets/{MainImagePath}") : null;

    public string Background => IsActive == true ? "White" : "LightGray";
}
