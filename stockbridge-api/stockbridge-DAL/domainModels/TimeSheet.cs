using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TimeSheet
{
    public int TimeSheetId { get; set; }

    public DateOnly Date { get; set; }

    public int ClientId { get; set; }

    public int StaffId { get; set; }

    public decimal Hours { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedDate { get; set; }
}
