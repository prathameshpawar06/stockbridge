using System.ComponentModel.DataAnnotations;

namespace stockbridge_DAL.DTOs
{
    public class TemplatePrincipalFilterRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int PageSize { get; set; }
        public string? SearchName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
