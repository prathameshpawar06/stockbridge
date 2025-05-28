using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TemplatePrincipal
{
    public int PrincipalId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual ICollection<TemplateMajor> TemplateMajors { get; set; } = new List<TemplateMajor>();
}
