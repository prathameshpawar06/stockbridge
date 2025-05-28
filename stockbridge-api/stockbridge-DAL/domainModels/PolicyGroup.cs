using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyGroup
{
    public int ClientId { get; set; }

    public int PolicyId { get; set; }

    public string? GroupLocationSequence { get; set; }

    public int? LocationId { get; set; }

    public string? Name { get; set; }

    public DateTime AddDate { get; set; }

    public string AddUid { get; set; } = null!;

    public DateTime ChangeDate { get; set; }

    public string ChangeUid { get; set; } = null!;
}
