using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyMinorDef
{
    public int MinorId { get; set; }

    public int ColumnDefId { get; set; }

    public int RowSequence { get; set; }

    public string ColumnValue { get; set; } = null!;

    public byte[]? TimeStamp { get; set; }

    public virtual PolicyMajorColDef ColumnDef { get; set; } = null!;

    public virtual PolicyDefinition? PolicyDefinition { get; set; }
}
