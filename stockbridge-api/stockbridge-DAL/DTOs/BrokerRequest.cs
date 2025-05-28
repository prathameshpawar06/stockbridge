namespace stockbridge_DAL.DTOs
{
    public class BrokerRequest
    {
        public int? BrokerId { get; set; }

        public string? Name { get; set; } = null!;

        public string? Address1 { get; set; }

        public string? Address2 { get; set; }

        public string? Address3 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Zip { get; set; }

        public string? Telephone { get; set; }

        public string? Fax { get; set; }
    }
}
