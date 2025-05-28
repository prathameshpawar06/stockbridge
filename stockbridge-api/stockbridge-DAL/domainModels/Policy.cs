using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class Policy
{
    public int PolicyId { get; set; }

    public int ClientId { get; set; }

    public int PrincipalId { get; set; }

    public int CarrierId { get; set; }

    public int? BrokerId { get; set; }

    public int StaffId { get; set; }

    public string PolicyNo { get; set; } = null!;

    public string PolicyTitle { get; set; } = null!;

    public string? PolicyComment { get; set; }

    public int? PrintSequence { get; set; }

    public DateTime AddDate { get; set; }

    public string AddUid { get; set; } = null!;

    public DateTime ChangeDate { get; set; }

    public string ChangeUid { get; set; } = null!;

    public DateTime? InceptionDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public bool Expired { get; set; }

    public double? AnualPremium { get; set; }

    public double? MinimumDeposit { get; set; }

    public double? MinimumEarned { get; set; }

    public bool Audit { get; set; }

    public string Status { get; set; } = null!;

    public bool SuppressNamedInsureds { get; set; }

    public bool SuppressLocations { get; set; }

    public bool SuppressEntities { get; set; }

    public bool SuppressLocationsNotScheduled { get; set; }

    public int PolicyType { get; set; }

    public int ParentPolicy { get; set; }

    public byte[]? TimeStamp { get; set; }

    public virtual Broker? Broker { get; set; }

    public virtual Carrier Carrier { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ClientStaff ClientStaff { get; set; } = null!;

    public virtual ICollection<PolicyEntity> PolicyEntities { get; set; } = new List<PolicyEntity>();

    public virtual ICollection<PolicyLocation> PolicyLocations { get; set; } = new List<PolicyLocation>();

    public virtual ICollection<PolicyMajor> PolicyMajors { get; set; } = new List<PolicyMajor>();

    public virtual TemplatePrincipal Principal { get; set; } = null!;
}
