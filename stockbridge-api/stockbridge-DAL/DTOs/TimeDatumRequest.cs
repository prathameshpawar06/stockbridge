using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class TimeDatumRequest
    {
        public int? TimeSheetId { get; set; }

        public DateOnly? Date { get; set; }

        public int? ClientId { get; set; }

        public int? StaffId { get; set; }

        public decimal? Hours { get; set; }

        public string? Notes { get; set; }

        public DateTime? CreatedDate { get; set; }
        public String? Email { get; set; }
    }
}
