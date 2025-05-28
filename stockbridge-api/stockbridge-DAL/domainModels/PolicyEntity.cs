using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class PolicyEntity
{
    public int PolicyEntityId { get; set; }

    public int PolicyId { get; set; }

    public int ClientId { get; set; }

    public int EntityId { get; set; }

    public bool NamedInsured { get; set; }

    public DateTime AddDate { get; set; }

    public string AddUid { get; set; } = null!;

    public DateTime ChangeDate { get; set; }

    public string ChangeUid { get; set; } = null!;

    public virtual ClientEntity Entity { get; set; } = null!;

    public virtual Policy Policy { get; set; } = null!;
}
