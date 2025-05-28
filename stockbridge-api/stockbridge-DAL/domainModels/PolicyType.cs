using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyType
{
    public int PolicyTypeId { get; set; }

    public string PolicyTypeName { get; set; } = null!;
}
