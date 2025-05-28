using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class TemplateDefinition
{
    public int DefinitionId { get; set; }

    public string Name { get; set; } = null!;

    public int MajorId { get; set; }

    public int MinorRowSequence { get; set; }

    public string DefinitionText { get; set; } = null!;

    public virtual ICollection<TemplateMinorDef> TemplateMinorDefs { get; set; } = new List<TemplateMinorDef>();
}
