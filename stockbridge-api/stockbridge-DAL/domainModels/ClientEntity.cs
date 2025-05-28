using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class ClientEntity
{
    public int EntityId { get; set; }

    public int ClientId { get; set; }

    public int? Sequence { get; set; }

    public string Name { get; set; } = null!;

    public string? Comments { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<PolicyEntity> PolicyEntities { get; set; } = new List<PolicyEntity>();
}
