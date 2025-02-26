using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class Payment
{
    public string PaymentId { get; set; } = null!;

    public string? PaymentName { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
