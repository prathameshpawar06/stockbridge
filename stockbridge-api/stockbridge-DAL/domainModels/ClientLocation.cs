using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class ClientLocation
{
    public int LocationId { get; set; }

    public int ClientId { get; set; }

    public int Sequence { get; set; }

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? PostalCode { get; set; }

    public string? Telephone { get; set; }

    public string? Fax { get; set; }

    public string? Description { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<PolicyLocation> PolicyLocations { get; set; } = new List<PolicyLocation>();
}
