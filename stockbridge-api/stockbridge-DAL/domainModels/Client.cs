using System;
using System.Collections.Generic;

namespace stockbridge_DAL.domainModels;

public partial class Client
{
    public int ClientId { get; set; }

    public string ClientAcctId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string? WebAddress { get; set; }

    public int PrimaryLocation { get; set; }

    public int PrimaryConsultant { get; set; }

    public int BillToContact { get; set; }

    public bool Active { get; set; }

    public bool SuppressLocations { get; set; }

    public bool RetainerAccount { get; set; }

    public double Retainer { get; set; }

    public int PaymentNumber { get; set; }

    public double PaymentAmount { get; set; }

    public string PaymentFrequency { get; set; } = null!;

    public DateTime? PaymentStart { get; set; }

    public DateTime? BilledThru { get; set; }

    public DateTime? AccountOpenDate { get; set; }

    public DateTime? AccountTerminateDate { get; set; }

    public string? LoginName { get; set; }

    public string? LoginPassword { get; set; }

    public string? Comments { get; set; }

    public byte[]? TimeStamp { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? PostalCode { get; set; }

    public string? Telephone { get; set; }

    public string? Fax { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<ClientContact> ClientContacts { get; set; } = new List<ClientContact>();

    public virtual ICollection<ClientEntity> ClientEntities { get; set; } = new List<ClientEntity>();

    public virtual ICollection<ClientLocation> ClientLocations { get; set; } = new List<ClientLocation>();

    public virtual ICollection<ClientStaff> ClientStaffs { get; set; } = new List<ClientStaff>();

    public virtual ICollection<InvoiceHeader> InvoiceHeaders { get; set; } = new List<InvoiceHeader>();

    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

    public virtual ICollection<TimeDatum> TimeData { get; set; } = new List<TimeDatum>();
}
