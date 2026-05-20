using System;
using System.Collections.Generic;

namespace Tema_3.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string PortionQuantity { get; set; } = null!;

    public decimal TotalQuantity { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Allergen> Allergens { get; set; } = new List<Allergen>();
}
