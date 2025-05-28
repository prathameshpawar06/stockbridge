using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyLocation
{
    public int PolicyId { get; set; }

    public int ClientId { get; set; }

    public int LocationId { get; set; }

    public DateTime AddDate { get; set; }

    public string AddUid { get; set; } = null!;

    public DateTime ChangeDate { get; set; }

    public string ChangeUid { get; set; } = null!;

    public virtual ClientLocation Location { get; set; } = null!;

    public virtual Policy Policy { get; set; } = null!;
}
