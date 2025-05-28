using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stockbridge_DAL.DTOs
{
    public class UpdatePolicyPrientSequenceRequest
    {
        public int ClientId { get; set; }

        public List<int> PolicyIdsList { get; set; } = new List<int>();
    }
}
