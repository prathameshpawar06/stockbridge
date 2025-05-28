using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class InvoiceHeader
{
    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    public int ClientId { get; set; }

    public DateTime OpeningDate { get; set; }

    public DateTime ClosingDate { get; set; }

    public double BalanceForward { get; set; }

    public string? Comment { get; set; }

    public double InvoiceAmount { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
