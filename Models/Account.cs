using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class Account
{
    public string AccountId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
