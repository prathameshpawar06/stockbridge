using AutoMapper;
using Microsoft.EntityFrameworkCore;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;
using stockbridge_DAL.IRepositories;

namespace stockbridge_DAL.Repositories
{
    public class CarrierRepository : ICarrierRepository
    {
        private readonly StockbridgeContext _context;
        private readonly IMapper _mapper;

        public CarrierRepository(StockbridgeContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Get Template Principal By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CarrierModel> GetCarrierById(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("Invalid Carrier ID.");
            }

            var carrier = await _context.Carriers.FirstOrDefaultAsync(x => x.CarrierId == id);
            var carrierData = _mapper.Map<CarrierModel>(carrier);

            return carrierData;
        }

        /// <summary>
        /// Add Carrier
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Carrier> AddCarrier(CarrierRequest model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var carrier = _mapper.Map<Carrier>(model);
                _context.Carriers.Add(carrier);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return carrier;
            }
            catch (Exception ex)
            {
                // Rollback in case of an error
                await transaction.RollbackAsync();
                throw ex;  // Rethrow the exception to be handled higher up
            }
        }

        /// <summary>
        /// Update Carrier
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Carrier> UpdateCarrier(CarrierRequest model)
        {
            if (model.CarrierId == null)
            {
                throw new ArgumentException("Invalid Carrier ID.");
            }

            int carrierId = model.CarrierId.Value;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var carrier = await _context.Carriers.FindAsync(carrierId);
                if (carrier == null)
                {
                    throw new KeyNotFoundException("Carrier not found.");
                }

                _mapper.Map(model, carrier);

                _context.Carriers.Update(carrier);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return carrier;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Delete carrier
        /// </summary>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCarrier(int carrierId)
        {
            try
            {
                var carrier = await _context.Carriers.FindAsync(carrierId);
                if (carrier == null)
                {
                    return false; 
                }

                // Remove the carrier itself
                _context.Carriers.Remove(carrier);

                // Save changes
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (Replace with actual logging)
                Console.WriteLine($"Error deleting carrier: {ex.Message}");
                return false;
            }
        }
    }
}
