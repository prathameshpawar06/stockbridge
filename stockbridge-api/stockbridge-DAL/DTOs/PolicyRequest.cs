namespace stockbridge_DAL.DTOs
{
    public class PolicyRequest
    {
        public int? PolicyId { get; set; }
        public int ClientId { get; set; }
        public int PrincipalId { get; set; }

        //Policy Title
        public string? PolicyNo { get; set; } = null!;
        public string? PolicyTitle { get; set; } = null!;
        public int? CarrierId { get; set; }
        public int? BrokerId { get; set; }
        public int? StaffId { get; set; }
        public string? PolicyComment { get; set; }

        //Policy Details
        public DateTime? InceptionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public double? AnualPremium { get; set; }
        public double? MinimumDeposit { get; set; }
        public double? MinimumEarned { get; set; }
        //StatusIsActive
        public bool Expired { get; set; }
        //PolicyIsAuditable
        public bool Audit { get; set; }

        public bool SuppressNamedInsureds { get; set; }

        public bool SuppressLocations { get; set; }

        public bool SuppressEntities { get; set; }

        public bool SuppressLocationsNotScheduled { get; set; }

        //Selected Entities
        public int[]? SelectedEntityId { get; set; }

        //Selected Locations
        public int[]? SelectedLocationId { get; set; }
        public ICollection<PolicyMajorModel>? PolicyMajors { get; set; } = new List<PolicyMajorModel>();

        //public IList<PolicyMajorModel> PolicyMajorsList { get; set; } = new List<PolicyMajorModel>();


    }

    public partial class PolicyMajorModel
    {
        public int? MajorId { get; set; }

        public int PolicyId { get; set; }

        public string? Name { get; set; } = null!;

        public string? Comments { get; set; }

        public int? Sequence { get; set; }

        public ICollection<PolicyMajorColDefModel>? PolicyMajorColDefs { get; set; } = new List<PolicyMajorColDefModel>();
    }

    public partial class PolicyMajorColDefModel
    {
        public int? ColumnDefId { get; set; }

        public int MajorId { get; set; }

        public int? Sequence { get; set; }

        public string? ColumnName { get; set; }

        public string? ColumnDescription { get; set; }

        public string? ColumnType { get; set; } = null!;

        //public string? NumericSign { get; set; }

        //public double? Width { get; set; }

        public string? Value { get; set; }

        //public byte[]? TimeStamp { get; set; }

        public ICollection<PolicyMinorDefModel>? PolicyMinorDefs { get; set; } = new List<PolicyMinorDefModel>();
    }


    public partial class PolicyMinorDefModel
    {
        public int? MinorId { get; set; }

        public int ColumnDefId { get; set; }

        public int? RowSequence { get; set; }

        public string? ColumnValue { get; set; } = null!;

        //public byte[]? TimeStamp { get; set; }

    }

}
