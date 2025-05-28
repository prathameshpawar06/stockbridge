using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class Broker
{
    public int BrokerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? Telephone { get; set; }

    public string? Fax { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual ICollection<BrokerContact> BrokerContacts { get; set; } = new List<BrokerContact>();

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
