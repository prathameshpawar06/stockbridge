using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using stockbridge_DAL.DTOs;
using stockbridge_DAL.IRepositories;

namespace stockbridge_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrokerController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PolicyController> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);
        private readonly IBrokerRepository _brokerRepository;

        public BrokerController(IMemoryCache cache,
            ILogger<PolicyController> logger,
            IBrokerRepository brokerRepository)
        {
            _cache = cache;
            _logger = logger;
            _brokerRepository = brokerRepository;
        }

        [HttpGet("GetBrokerById/{id}")]
        public async Task<ActionResult<PaginatedResult<BrokerModel>>> GetBrokerById(int id)
        {
            try
            {
                string cacheKey = $"broker_{id}";

                if (!_cache.TryGetValue(cacheKey, out BrokerModel broker))
                {
                    broker = await _brokerRepository.GetBrokerById(id);

                    if (broker == null)
                    {
                        _logger.LogWarning("Broker with ID {id} not found.", id);
                        return NotFound($"Broker with ID {id} not found.");
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    };

                    _cache.Set(cacheKey, broker, cacheOptions);
                }

                return Ok(broker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving broker with ID {brokerId}.", id);
                return StatusCode(500, "An error occurred while retrieving the broker. Please try again later.");
            }
        }

        [HttpPost]
        [Route("AddBroker")]
        public async Task<IActionResult> AddBroker([FromBody] BrokerRequest model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid data received." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid model data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                if (model.BrokerId > 0)
                {
                    var updatedBroker = await _brokerRepository.UpdateBroker(model);
                    if (updatedBroker != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"broker_{updatedBroker.BrokerId}";
                        _cache.Set(cacheKey, updatedBroker, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Broker updated successfully." });
                }
                else
                {
                    await _brokerRepository.AddBroker(model);
                }

                return Ok(new { issuccess = true, message = "Broker created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the Broker.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while creating the Broker. Please try again later." });
            }
        }

        [HttpDelete]
        [Route("DeleteBroker/{brokerId}")]
        public async Task<IActionResult> DeleteBroker(int brokerId)
        {
            try
            {
                if (brokerId <= 0)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid brokerId ID." });
                }

                var result = await _brokerRepository.DeleteBroker(brokerId);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "Broker not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "Broker deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the Broker.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the Broker. Please try again later." });
            }
        }
    }
}
