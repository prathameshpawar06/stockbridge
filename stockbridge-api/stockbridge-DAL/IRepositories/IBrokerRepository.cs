using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;

namespace stockbridge_DAL.IRepositories
{
    public interface IBrokerRepository
    {
        /// <summary>
        /// Get broker by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BrokerModel> GetBrokerById(int id);

        /// <summary>
        /// Add broker
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Broker> AddBroker(BrokerRequest model);

        /// <summary>
        /// Update Broker
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Broker> UpdateBroker(BrokerRequest model);

        /// <summary>
        /// Delete broker
        /// </summary>
        /// <param name="brokerId"></param>
        /// <returns></returns>
        Task<bool> DeleteBroker(int brokerId);
    }
}
