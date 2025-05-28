using stockbridge_DAL.domainModels;

namespace stockbridge_DAL.DTOs
{
    public class StaffViewModel : Staff
    {
        public List<Client>? ClientsList { get; set; } = new List<Client>();
    }
}
