namespace stockbridge_DAL.DTOs
{
    public class PolicyExpirationRequest
    {
        public int PolicyId { get; set; }

        public bool? IsExpired { get; set; } = false;

        public bool? RenewPolicy { get; set; } = false;
    }
}
