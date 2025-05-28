using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TemplateMajor
{
    public int MajorId { get; set; }

    public int PrincipalId { get; set; }

    public string Name { get; set; } = null!;

    public string? Comments { get; set; }

    public int? Sequence { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual TemplatePrincipal Principal { get; set; } = null!;

    public virtual ICollection<TemplateMajorColDef> TemplateMajorColDefs { get; set; } = new List<TemplateMajorColDef>();
}
