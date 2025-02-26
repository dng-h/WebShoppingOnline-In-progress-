using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class OrderDetail
{
    public string OrderDetailId { get; set; } = null!;

    public string? OrderId { get; set; }

    public string? ProductId { get; set; }

    public decimal? Price { get; set; }

    public int? Amount { get; set; }

    public decimal? Total { get; set; }

    public string? Note { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
