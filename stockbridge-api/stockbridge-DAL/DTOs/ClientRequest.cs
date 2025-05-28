namespace stockbridge_DAL.DTOs
{
    public class ClientRequest
    {
        public BasicTab? BasicTab { get; set; }
        public List<Contact>? Contacts { get; set; }
        public List<Location>? Locations { get; set; }
        public List<Entities>? Entities { get; set; }
        public List<StaffModel>? Staff { get; set; }
    }

    public class BasicTab
    {
        public int? ClientId { get; set; }
        public string? ClientAcctId { get; set; }
        public string? CompanyName { get; set; }
        public bool? Active { get; set; }
        public bool? SuppressLocations { get; set; }
        public string? Address1 { get; set; }
        //public string? BusinessPhone { get; set; }
        public string? Telephone { get; set; }
        public string? Address2 { get; set; }
        //Not in client
        public string? Email { get; set; }
        public string? City { get; set; }
        public int? BillToContact { get; set; }
        public string? State { get; set; }
        public int? PrimaryConsultant { get; set; }
        public string? PostalCode { get; set; }
        //InceptionDate
        public DateTime? AccountOpenDate { get; set; }
        public string? Country { get; set; }
        //ExpirationDate
        public DateTime? AccountTerminateDate { get; set; }
        public bool RetainerAccount { get; set; }
        public decimal? Retainer { get; set; }
        public DateTime? PaymentStart { get; set; }
        public string? PaymentFrequency { get; set; }
        public DateTime? BilledThru { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? Comments { get; set; }
    }

    public class Contact
    {
        public int? ContactId { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? NamePrefix { get; set; }
        public string? JobTitle { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? WebAddress { get; set; }
    }

    public class Location
    {
        public int? LocationId { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public int Sequence { get; set; }
    }

    public class Entities
    {
        public int? EntityId { get; set; }
        public string? Name { get; set; }
        public string? Comments { get; set; }
    }

    public class StaffModel
    {
        public int? ClientId { get; set; }
        public int? StaffId { get; set; }
        public string? Status { get; set; }
        public double? Rate { get; set; }
        public string? Name { get; set; }
    }

}
