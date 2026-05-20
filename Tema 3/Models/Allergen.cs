using System;
using System.Collections.Generic;

namespace Tema_3.Models;

public partial class Allergen
{
    public int AllergenId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
