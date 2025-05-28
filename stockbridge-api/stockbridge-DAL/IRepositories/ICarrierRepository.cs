using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;

namespace stockbridge_DAL.IRepositories
{
    public interface ICarrierRepository
    {
        /// <summary>
        /// Get carrier by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CarrierModel> GetCarrierById(int id);

        /// <summary>
        /// Add Carrier
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Carrier> AddCarrier(CarrierRequest model);

        /// <summary>
        /// Update Carrier
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        Task<Carrier> UpdateCarrier(CarrierRequest model);

        /// <summary>
        /// Delete Carrier
        /// </summary>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        Task<bool> DeleteCarrier(int carrierId);
    }
}

