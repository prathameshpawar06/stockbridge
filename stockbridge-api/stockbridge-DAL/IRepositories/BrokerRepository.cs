using AutoMapper;
using Microsoft.EntityFrameworkCore;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;

namespace stockbridge_DAL.IRepositories
{
    public class BrokerRepository : IBrokerRepository
    {
        private readonly StockbridgeContext _context;
        private readonly IMapper _mapper;

        public BrokerRepository(StockbridgeContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Get brokers by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<BrokerModel> GetBrokerById(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("Invalid Carrier ID.");
            }

            var broker = await _context.Brokers.FirstOrDefaultAsync(x => x.BrokerId == id);
            var brokerData = _mapper.Map<BrokerModel>(broker);

            return brokerData;
        }

        /// <summary>
        /// Add broker
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Broker> AddBroker(BrokerRequest model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var broker = _mapper.Map<Broker>(model);
                _context.Brokers.Add(broker);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return broker;
            }
            catch (Exception ex)
            {
                // Rollback in case of an error
                await transaction.RollbackAsync();
                throw ex;  // Rethrow the exception to be handled higher up
            }
        }

        /// <summary>
        /// update broker
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Broker> UpdateBroker(BrokerRequest model)
        {
            if (model.BrokerId == null)
            {
                throw new ArgumentException("Invalid Broker ID.");
            }

            int brokerId = model.BrokerId.Value;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var broker = await _context.Brokers.FindAsync(brokerId);
                if (broker == null)
                {
                    throw new KeyNotFoundException("Broker not found.");
                }

                _mapper.Map(model, broker);

                _context.Brokers.Update(broker);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return broker;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        /// <summary>
        /// Delete broker
        /// </summary>
        /// <param name="brokerId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteBroker(int brokerId)
        {
            try
            {
                var broker = await _context.Brokers.FindAsync(brokerId);
                if (broker == null)
                {
                    return false;
                }

                // Remove the broker itself
                _context.Brokers.Remove(broker);

                // Save changes
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (Replace with actual logging)
                Console.WriteLine($"Error deleting broker: {ex.Message}");
                return false;
            }
        }
    }
}
