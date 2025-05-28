using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class StaffRequest : Staff
    {
        public List<int>? ClientIds { get; set; }
    }
}
