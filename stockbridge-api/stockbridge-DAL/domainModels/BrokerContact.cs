using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class BrokerContact
{
    public int ContactId { get; set; }

    public int BrokerId { get; set; }

    public string LastName { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? NamePrefix { get; set; }

    public string? JobTitle { get; set; }

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? ZipCode { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Pager { get; set; }

    public string? Ext { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? WebAddress { get; set; }

    public string? LoginName { get; set; }

    public string? LoginPassword { get; set; }

    public string? Comments { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual Broker Broker { get; set; } = null!;
}
