using System;
using System.Collections.Generic;

namespace WebShoppingOnline.Models;

public partial class ProductImage
{
    public string ImageId { get; set; } = null!;
    public string? ProductId { get; set; }

    public string? Image { get; set; }

}
