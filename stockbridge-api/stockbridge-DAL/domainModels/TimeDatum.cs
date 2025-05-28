using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TimeDatum
{
    public int EntryId { get; set; }

    public int ClientId { get; set; }

    public int StaffId { get; set; }

    public DateTime DateOfService { get; set; }

    public DateTime TotalTime { get; set; }

    public bool Visit { get; set; }

    public double StaffRate { get; set; }

    public bool Billable { get; set; }

    public string? InvoiceNumber { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
