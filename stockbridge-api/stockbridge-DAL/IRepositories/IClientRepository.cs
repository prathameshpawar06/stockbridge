using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stockbridge_DAL.Repositories
{
    public interface IClientRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<PaginatedResult<ClientDTO>> GetClientsPaginatedAsync(ClientFilterRequest clientFilterRequest);


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<int> GetTotalClientsCountAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<ClientDTO> GetClientByIdAsync(int clientId);

        /// <summary>
        /// Add client to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Client> AddClientAsync(ClientRequest model);

        /// <summary>
        /// Update an existing client in the database
        /// </summary>
        /// <param name="clientId">The ID of the client to update</param>
        /// <param name="model">The updated client data</param>
        /// <returns></returns>
        Task<Client> UpdateClientAsync(ClientRequest model);

        /// <summary>
        /// To get all Staff
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedResult<Staff>> GetStaffAsync(string name = null, int pageNumber = 1, int pageSize = 100, int? clientId = null, bool? isActive = false, bool? IsNonActive = false);

        /// <summary>
        /// Delete the client
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<bool> DeleteClient(int clientId);

        /// <summary>
        /// Get the staff by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<StaffViewModel> GetStaffByIdAsync(int id);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedResult<ClientContact>> GetClientContacts(ClientContactFilterRequest clientContactFilterRequestObj);
        /// <summary>
        /// Update staff to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Staff> UpdateStaff(StaffRequest model);  
        
       /// <summary>
       /// 
       /// </summary>
       /// <param name="model"></param>
       /// <returns></returns>
        Task<bool> UpdateTimeData(List<TimeDatumRequest> modelList);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        Task<List<TimeSheet>> GetTimeListData(int staffId);

        /// <summary>
        /// Add staff to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Staff> AddStaff(StaffRequest model);

        /// <summary>
        /// Get staffId by email
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        Task<int> GetStaffIdByEmail(string email);
    }
}
