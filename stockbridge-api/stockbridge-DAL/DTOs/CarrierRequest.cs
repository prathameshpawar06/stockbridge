using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class CarrierRequest
    {
        public int? CarrierId { get; set; }

        public string? Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? AmBest { get; set; }

        public bool? Licensed { get; set; }
    }
}
