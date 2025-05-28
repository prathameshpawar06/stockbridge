using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyMajor
{
    public int MajorId { get; set; }

    public int PolicyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Comments { get; set; }

    public int? Sequence { get; set; }

    public virtual Policy Policy { get; set; } = null!;

    public virtual ICollection<PolicyMajorColDef> PolicyMajorColDefs { get; set; } = new List<PolicyMajorColDef>();
}
