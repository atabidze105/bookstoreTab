using System;
using System.Collections.Generic;

namespace bookshopTab.Models;

public partial class Productphoto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string PhotoPath { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
