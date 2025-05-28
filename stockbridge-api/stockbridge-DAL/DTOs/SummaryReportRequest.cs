namespace stockbridge_DAL.DTOs
{
    public class SummaryReportRequest
    {
        public int ClientId { get; set; }
        public List<int> PolicyIds { get; set; } = new List<int>();
    }
}
