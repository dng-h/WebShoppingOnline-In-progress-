using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class Cart
{
    public string CartId { get; set; } = null!;

    public string? AccountId { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
}
