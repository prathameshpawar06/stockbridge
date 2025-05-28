using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyMajorColDef
{
    public int ColumnDefId { get; set; }

    public int MajorId { get; set; }

    public int Sequence { get; set; }

    public string? ColumnName { get; set; }

    public string? ColumnDescription { get; set; }

    public string ColumnType { get; set; } = null!;

    public string? NumericSign { get; set; }

    public double Width { get; set; }

    public string? Value { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual PolicyMajor Major { get; set; } = null!;

    public virtual ICollection<PolicyMinorDef> PolicyMinorDefs { get; set; } = new List<PolicyMinorDef>();
}
