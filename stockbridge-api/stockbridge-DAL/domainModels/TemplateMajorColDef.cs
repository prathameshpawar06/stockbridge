using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TemplateMajorColDef
{
    public int ColumnDefId { get; set; }

    public int MajorId { get; set; }

    public int Sequence { get; set; }

    public string? ColumnName { get; set; }

    public string? ColumnDescription { get; set; }

    public string ColumnType { get; set; } = null!;

    public string? NumericSign { get; set; }

    public int Width { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual TemplateMajor Major { get; set; } = null!;

    public virtual ICollection<TemplateMinorDef> TemplateMinorDefs { get; set; } = new List<TemplateMinorDef>();
}
