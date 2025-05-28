using System.ComponentModel.DataAnnotations;

namespace stockbridge_DAL.DTOs
{
    public class CarrierFilterRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [Range(-1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int PageSize { get; set; }

        public string? SearchQuery { get; set; }

        public bool ForStarting { get; set; } = false;
    }
}
