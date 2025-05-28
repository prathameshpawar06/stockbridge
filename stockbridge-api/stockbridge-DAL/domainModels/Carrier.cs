using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class Carrier
{
    public int CarrierId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? AmBest { get; set; }

    public bool Licensed { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
