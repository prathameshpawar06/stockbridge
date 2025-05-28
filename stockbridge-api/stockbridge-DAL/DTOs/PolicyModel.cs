using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class PolicyModel : Policy
    {
        public PolicyModel()
        {
            ClientLocations = new List<ClientLocation>();
            ClientEntities = new List<ClientEntity>();
        }
        //public PolicyMajor? PolicyMajor { get; set; }
        public List<ClientLocation>? ClientLocations { get; set; }
        public List<ClientEntity>? ClientEntities { get; set; }
        public string? PolicyTypeName { get; set; }
    }
}
