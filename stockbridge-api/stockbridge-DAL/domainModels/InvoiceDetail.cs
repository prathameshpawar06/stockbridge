using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class InvoiceDetail
{
    public int InvoiceDetailId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public double Quantity { get; set; }

    public string Description { get; set; } = null!;

    public double Rate { get; set; }

    public double LineTotal { get; set; }

    public virtual InvoiceHeader InvoiceNumberNavigation { get; set; } = null!;
}
