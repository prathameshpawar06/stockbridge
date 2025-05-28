using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class ClientStaff
{
    public int ClientId { get; set; }

    public int StaffId { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual Staff Staff { get; set; } = null!;
}
