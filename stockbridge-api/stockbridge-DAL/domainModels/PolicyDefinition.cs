using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyDefinition
{
    public int MinorId { get; set; }

    public int DefinitionId { get; set; }

    public string Name { get; set; } = null!;

    public string DefinitionText { get; set; } = null!;

    public virtual PolicyMinorDef Minor { get; set; } = null!;
}
