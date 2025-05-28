using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class Staff
{
    public int StaffId { get; set; }

    public string Name { get; set; } = null!;

    public double Rate { get; set; }

    public string? Class { get; set; }

    public string? Comments { get; set; }

    public string? Status { get; set; }

    public DateTime? TerminationDate { get; set; }

    public string? StaffOldId { get; set; }

    public byte[]? TimeStamp { get; set; }
    public string? Email { get; set; }

    public virtual ICollection<ClientStaff> ClientStaffs { get; set; } = new List<ClientStaff>();

    public virtual ICollection<TimeDatum> TimeData { get; set; } = new List<TimeDatum>();
}
