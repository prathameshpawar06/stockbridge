using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class PolicyMasterListModel : Policy
    {
        public string PolicyTypeName { get; set; } = null!;

    }
}
