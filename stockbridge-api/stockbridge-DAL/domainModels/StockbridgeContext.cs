using Microsoft.EntityFrameworkCore;

namespace stockbridge_DAL.domainModels;

public partial class StockbridgeContext : DbContext
{
    public StockbridgeContext()
    {
    }

    public StockbridgeContext(DbContextOptions<StockbridgeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Broker> Brokers { get; set; }

    public virtual DbSet<BrokerContact> BrokerContacts { get; set; }

    public virtual DbSet<Carrier> Carriers { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientContact> ClientContacts { get; set; }

    public virtual DbSet<ClientEntity> ClientEntities { get; set; }

    public virtual DbSet<ClientLocation> ClientLocations { get; set; }

    public virtual DbSet<ClientStaff> ClientStaffs { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<InvoiceHeader> InvoiceHeaders { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PolicyDefinition> PolicyDefinitions { get; set; }

    public virtual DbSet<PolicyEntity> PolicyEntities { get; set; }

    public virtual DbSet<PolicyGroup> PolicyGroups { get; set; }

    public virtual DbSet<PolicyLocation> PolicyLocations { get; set; }

    public virtual DbSet<PolicyMajor> PolicyMajors { get; set; }

    public virtual DbSet<PolicyMajorColDef> PolicyMajorColDefs { get; set; }

    public virtual DbSet<PolicyMinorDef> PolicyMinorDefs { get; set; }

    public virtual DbSet<PolicyType> PolicyTypes { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<TemplateDefinition> TemplateDefinitions { get; set; }

    public virtual DbSet<TemplateMajor> TemplateMajors { get; set; }

    public virtual DbSet<TemplateMajorColDef> TemplateMajorColDefs { get; set; }

    public virtual DbSet<TemplateMinorDef> TemplateMinorDefs { get; set; }

    public virtual DbSet<TemplatePrincipal> TemplatePrincipals { get; set; }

    public virtual DbSet<TimeDatum> TimeData { get; set; }
    public virtual DbSet<TimeSheet> TimeSheet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=stockbridgesql.database.windows.net;Database=stockbridge-db;User ID=stockbridgeadmin;Password=Hosting@123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimeSheet>(entity =>
        {
            entity.HasKey(e => e.TimeSheetId).HasName("PK__TimeShee__0625574A1E93CD90");

            entity.ToTable("TimeSheet");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Hours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<Broker>(entity =>
        {
            entity.HasKey(e => e.BrokerId).IsClustered(false);

            entity.ToTable("Broker");

            entity.Property(e => e.BrokerId).HasColumnName("BrokerID");
            entity.Property(e => e.Address1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address3)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Telephone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Zip)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BrokerContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).IsClustered(false);

            entity.ToTable("BrokerContact");

            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.Address1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BrokerId).HasColumnName("BrokerID");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("USA");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Ext)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.JobTitle)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoginName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoginPassword)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mobile)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.NamePrefix)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Pager)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.WebAddress)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ZipCode)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Broker).WithMany(p => p.BrokerContacts)
                .HasForeignKey(d => d.BrokerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BrokerContact_Broker");
        });

        modelBuilder.Entity<Carrier>(entity =>
        {
            entity.HasKey(e => e.CarrierId).IsClustered(false);

            entity.ToTable("Carrier");

            entity.Property(e => e.CarrierId).HasColumnName("CarrierID");
            entity.Property(e => e.AmBest)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).IsClustered(false);

            entity.ToTable("Client", tb => tb.HasTrigger("trgClient_Update"));

            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.AccountOpenDate).HasColumnType("datetime");
            entity.Property(e => e.AccountTerminateDate).HasColumnType("datetime");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Address1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("address2");
            entity.Property(e => e.BilledThru).HasColumnType("datetime");
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ClientAcctId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CompanyName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LoginName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoginPassword)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PaymentFrequency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.PaymentStart).HasColumnType("datetime");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Telephone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.WebAddress)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ClientContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).IsClustered(false);

            entity.ToTable("ClientContact");

            entity.Property(e => e.ContactId).HasColumnName("ContactID");
            entity.Property(e => e.Address1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Ext)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.JobTitle)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.LoginName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoginPassword)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mobile)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.NamePrefix)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Pager)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(14)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.WebAddress)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Client).WithMany(p => p.ClientContacts)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientContact_Client");
        });

        modelBuilder.Entity<ClientEntity>(entity =>
        {
            entity.HasKey(e => e.EntityId).IsClustered(false);

            entity.ToTable("ClientEntity");

            entity.Property(e => e.EntityId).HasColumnName("EntityID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Client).WithMany(p => p.ClientEntities)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientEntity_Client");
        });

        modelBuilder.Entity<ClientLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).IsClustered(false);

            entity.ToTable("ClientLocation");

            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Address1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address2)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("USA");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Telephone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Client).WithMany(p => p.ClientLocations)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientLocation_Client");
        });

        modelBuilder.Entity<ClientStaff>(entity =>
        {
            entity.HasKey(e => new { e.ClientId, e.StaffId }).IsClustered(false);

            entity.ToTable("ClientStaff");

            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientStaffs)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientStaff_Client");

            entity.HasOne(d => d.Staff).WithMany(p => p.ClientStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientStaff_Staff");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoiceDetailId).IsClustered(false);

            entity.ToTable("InvoiceDetail");

            entity.Property(e => e.InvoiceDetailId).HasColumnName("InvoiceDetailID");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.InvoiceNumberNavigation).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDetail_InvoiceHeader");
        });

        modelBuilder.Entity<InvoiceHeader>(entity =>
        {
            entity.HasKey(e => e.InvoiceNumber).IsClustered(false);

            entity.ToTable("InvoiceHeader");

            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ClosingDate).HasColumnType("datetime");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.OpeningDate).HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.InvoiceHeaders)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceHeader_Client");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).IsClustered(false);

            entity.ToTable("Policy", tb =>
                {
                    tb.HasTrigger("trgPolicyInsert");
                    tb.HasTrigger("trgPolicyUpdate");
                });

            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
            entity.Property(e => e.AddDate).HasColumnType("datetime");
            entity.Property(e => e.AddUid)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AddUID");
            entity.Property(e => e.BrokerId).HasColumnName("BrokerID");
            entity.Property(e => e.CarrierId).HasColumnName("CarrierID");
            entity.Property(e => e.ChangeDate).HasColumnType("datetime");
            entity.Property(e => e.ChangeUid)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ChangeUID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.InceptionDate).HasColumnType("datetime");
            entity.Property(e => e.PolicyComment)
                .HasMaxLength(5000)
                .IsUnicode(false);
            entity.Property(e => e.PolicyNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PolicyTitle)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PolicyType).HasDefaultValue(1);
            entity.Property(e => e.PrincipalId).HasColumnName("PrincipalID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("A");
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Broker).WithMany(p => p.Policies)
                .HasForeignKey(d => d.BrokerId)
                .HasConstraintName("FK_Policy_Broker");

            entity.HasOne(d => d.Carrier).WithMany(p => p.Policies)
                .HasForeignKey(d => d.CarrierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Policy_Carrier");

            entity.HasOne(d => d.Client).WithMany(p => p.Policies)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Policy_Client");

            entity.HasOne(d => d.Principal).WithMany(p => p.Policies)
                .HasForeignKey(d => d.PrincipalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Policy_Principal");

            entity.HasOne(d => d.ClientStaff).WithMany(p => p.Policies)
                .HasForeignKey(d => new { d.ClientId, d.StaffId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Policy_ClientStaff");
        });

        modelBuilder.Entity<PolicyDefinition>(entity =>
        {
            entity.HasKey(e => e.MinorId).IsClustered(false);

            entity.Property(e => e.MinorId)
                .ValueGeneratedNever()
                .HasColumnName("MinorID");
            entity.Property(e => e.DefinitionId).HasColumnName("DefinitionID");
            entity.Property(e => e.DefinitionText)
                .HasMaxLength(7000)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Minor).WithOne(p => p.PolicyDefinition)
                .HasForeignKey<PolicyDefinition>(d => d.MinorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyDefinitions_PolicyMinorDefs");
        });

        modelBuilder.Entity<PolicyEntity>(entity =>
        {
            entity.HasKey(e => e.PolicyEntityId).IsClustered(false);

            entity.ToTable("PolicyEntity", tb =>
                {
                    tb.HasTrigger("trgInsertPolicyEntity");
                    tb.HasTrigger("trgUpdatePolicyEntity");
                });

            entity.Property(e => e.PolicyEntityId).HasColumnName("PolicyEntityID");
            entity.Property(e => e.AddDate).HasColumnType("datetime");
            entity.Property(e => e.AddUid)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AddUID");
            entity.Property(e => e.ChangeDate).HasColumnType("datetime");
            entity.Property(e => e.ChangeUid)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ChangeUID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.EntityId).HasColumnName("EntityID");
            entity.Property(e => e.NamedInsured).HasDefaultValue(true);
            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");

            entity.HasOne(d => d.Entity).WithMany(p => p.PolicyEntities)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyEntity_ClientEntity");

            entity.HasOne(d => d.Policy).WithMany(p => p.PolicyEntities)
                .HasForeignKey(d => d.PolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyEntity_Policy");
        });

        modelBuilder.Entity<PolicyGroup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PolicyGroup", tb =>
                {
                    tb.HasTrigger("trgInsertPolicyGroup");
                    tb.HasTrigger("trgUpdatePolicyGroup");
                });

            entity.Property(e => e.AddDate).HasColumnType("datetime");
            entity.Property(e => e.AddUid)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AddUID");
            entity.Property(e => e.ChangeDate).HasColumnType("datetime");
            entity.Property(e => e.ChangeUid)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ChangeUID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.GroupLocationSequence)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
        });

        modelBuilder.Entity<PolicyLocation>(entity =>
        {
            entity.HasKey(e => new { e.PolicyId, e.ClientId, e.LocationId });

            entity.ToTable("PolicyLocation", tb =>
                {
                    tb.HasTrigger("trgInsertPolicyLocation");
                    tb.HasTrigger("trgUpdatedPolicyLocation");
                });

            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.AddDate).HasColumnType("datetime");
            entity.Property(e => e.AddUid)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("AddUID");
            entity.Property(e => e.ChangeDate).HasColumnType("datetime");
            entity.Property(e => e.ChangeUid)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ChangeUID");

            entity.HasOne(d => d.Location).WithMany(p => p.PolicyLocations)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyLocation_ClientLocation");

            entity.HasOne(d => d.Policy).WithMany(p => p.PolicyLocations)
                .HasForeignKey(d => d.PolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyLocation_Policy");
        });

        modelBuilder.Entity<PolicyMajor>(entity =>
        {
            entity.HasKey(e => e.MajorId).IsClustered(false);

            entity.ToTable("PolicyMajor", tb => tb.HasTrigger("trgInsertPolicyMajor"));

            entity.Property(e => e.MajorId).HasColumnName("MajorID");
            entity.Property(e => e.Comments)
                .HasMaxLength(5000)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");

            entity.HasOne(d => d.Policy).WithMany(p => p.PolicyMajors)
                .HasForeignKey(d => d.PolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyMajor_Policy");
        });

        modelBuilder.Entity<PolicyMajorColDef>(entity =>
        {
            entity.HasKey(e => e.ColumnDefId).IsClustered(false);

            entity.Property(e => e.ColumnDefId).HasColumnName("ColumnDefID");
            entity.Property(e => e.ColumnDescription)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ColumnName)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.ColumnType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MajorId).HasColumnName("MajorID");
            entity.Property(e => e.NumericSign)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Value)
                .HasMaxLength(5000)
                .IsUnicode(false);
            entity.Property(e => e.Width).HasDefaultValue(1.0);

            entity.HasOne(d => d.Major).WithMany(p => p.PolicyMajorColDefs)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyMajorColumnDefs_PolicyMajor");
        });

        modelBuilder.Entity<PolicyMinorDef>(entity =>
        {
            entity.HasKey(e => e.MinorId).IsClustered(false);

            entity.Property(e => e.MinorId).HasColumnName("MinorID");
            entity.Property(e => e.ColumnDefId).HasColumnName("ColumnDefID");
            entity.Property(e => e.ColumnValue)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ColumnDef).WithMany(p => p.PolicyMinorDefs)
                .HasForeignKey(d => d.ColumnDefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PolicyMinorDefs_PolicyMajorColDefs");
        });

        modelBuilder.Entity<PolicyType>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PolicyType");

            entity.Property(e => e.PolicyTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("PolicyTypeID");
            entity.Property(e => e.PolicyTypeName)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).IsClustered(false);

            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Class)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.StaffOldId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("StaffOldID");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("A");
            entity.Property(e => e.TerminationDate).HasColumnType("datetime");
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TemplateDefinition>(entity =>
        {
            entity.HasKey(e => e.DefinitionId).IsClustered(false);

            entity.Property(e => e.DefinitionId).HasColumnName("DefinitionID");
            entity.Property(e => e.DefinitionText)
                .HasMaxLength(7000)
                .IsUnicode(false);
            entity.Property(e => e.MajorId).HasColumnName("MajorID");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TemplateMajor>(entity =>
        {
            entity.HasKey(e => e.MajorId).IsClustered(false);

            entity.ToTable("TemplateMajor");

            entity.Property(e => e.MajorId).HasColumnName("MajorID");
            entity.Property(e => e.Comments)
                .HasMaxLength(5000)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrincipalId).HasColumnName("PrincipalID");
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Principal).WithMany(p => p.TemplateMajors)
                .HasForeignKey(d => d.PrincipalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TemplateMajor_TemplatePrincipal");
        });

        modelBuilder.Entity<TemplateMajorColDef>(entity =>
        {
            entity.HasKey(e => e.ColumnDefId).IsClustered(false);

            entity.Property(e => e.ColumnDefId).HasColumnName("ColumnDefID");
            entity.Property(e => e.ColumnDescription)
                .HasMaxLength(5000)
                .IsUnicode(false);
            entity.Property(e => e.ColumnName)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.ColumnType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MajorId).HasColumnName("MajorID");
            entity.Property(e => e.NumericSign)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Width).HasDefaultValue(1);

            entity.HasOne(d => d.Major).WithMany(p => p.TemplateMajorColDefs)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TemplateMajorColumnDefs_TemplateMajor");
        });

        modelBuilder.Entity<TemplateMinorDef>(entity =>
        {
            entity.HasKey(e => e.MinorId).IsClustered(false);

            entity.Property(e => e.MinorId).HasColumnName("MinorID");
            entity.Property(e => e.ColumnDefId).HasColumnName("ColumnDefID");
            entity.Property(e => e.ColumnValue)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.DefinitionId).HasColumnName("DefinitionID");
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ColumnDef).WithMany(p => p.TemplateMinorDefs)
                .HasForeignKey(d => d.ColumnDefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TemplateMinorDefs_TemplateMajorColDefs");

            entity.HasOne(d => d.Definition).WithMany(p => p.TemplateMinorDefs)
                .HasForeignKey(d => d.DefinitionId)
                .HasConstraintName("FK_TemplateMinorDefs_TemplateDefinitions");
        });

        modelBuilder.Entity<TemplatePrincipal>(entity =>
        {
            entity.HasKey(e => e.PrincipalId).IsClustered(false);

            entity.ToTable("TemplatePrincipal");

            entity.Property(e => e.PrincipalId).HasColumnName("PrincipalID");
            entity.Property(e => e.Description)
                .HasMaxLength(5000)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TimeDatum>(entity =>
        {
            entity.HasKey(e => e.EntryId).IsClustered(false);

            entity.Property(e => e.EntryId).HasColumnName("EntryID");
            entity.Property(e => e.Billable).HasDefaultValue(true);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.DateOfService).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.TotalTime).HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.TimeData)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimeData_Client");

            entity.HasOne(d => d.Staff).WithMany(p => p.TimeData)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimeData_Staff");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
