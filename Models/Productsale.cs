using System;
using System.Collections.Generic;

namespace bookshopTab.Models;

public partial class Productsale
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public DateOnly Date { get; set; }

    public int Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;
}
