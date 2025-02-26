using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class CartDetail
{
    public string CartDetailId { get; set; } = null!;

    public string? CartId { get; set; }

    public string? ProductId { get; set; }

    public int? Quantity { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Product? Product { get; set; }
}
