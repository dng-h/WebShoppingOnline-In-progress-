using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class Product
{
    public string ProductId { get; set; } = null!;

    public string? ProductName { get; set; }

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Image { get; set; }

    public string? CategoryId { get; set; }

    public int? Amount { get; set; }

    public string? SupplierId { get; set; }

    public int? Discount { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Supplier? Supplier { get; set; }
}
