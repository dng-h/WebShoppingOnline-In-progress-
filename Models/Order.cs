using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string? AccountId { get; set; }

    public string? OrderType { get; set; }

    public string? PaymentId { get; set; }

    public string? RecipientName { get; set; }

    public string? RecipientPhone { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Status { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ShippingDate { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Payment? Payment { get; set; }
}
