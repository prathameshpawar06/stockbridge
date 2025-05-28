using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class PolicyListModel : Policy
    {
        public PolicyListModel()
        {
        }
        public int PolicyId { get; set; }
        public string PolicyNo { get; set; } = null!;

        public string PolicyTitle { get; set; } = null!;

        public string? PolicyComment { get; set; }
        public string? Description { get; set; }

        public string? PolicyTypeName { get; set; }
    }
}
