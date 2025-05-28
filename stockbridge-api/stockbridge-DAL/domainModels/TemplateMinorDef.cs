using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TemplateMinorDef
{
    public int MinorId { get; set; }

    public int ColumnDefId { get; set; }

    public int RowSequence { get; set; }

    public string ColumnValue { get; set; } = null!;

    public int? DefinitionId { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual TemplateMajorColDef ColumnDef { get; set; } = null!;

    public virtual TemplateDefinition? Definition { get; set; }
}
