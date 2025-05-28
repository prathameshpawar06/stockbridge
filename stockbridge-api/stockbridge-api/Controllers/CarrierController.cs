using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using stockbridge_DAL.DTOs;
using stockbridge_DAL.IRepositories;

namespace stockbridge_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarrierController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PolicyController> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);
        public readonly ICarrierRepository _carrierRepository;

        public CarrierController(IMemoryCache cache,
            ILogger<PolicyController> logger,
            ICarrierRepository carrierRepository)
        {
            _cache = cache;
            _logger = logger;
            _carrierRepository = carrierRepository;
        }

        [HttpGet("GetCarrierById/{id}")]
        public async Task<ActionResult<PaginatedResult<CarrierModel>>> GetCarrierById(int id)
        {
            try
            {
                string cacheKey = $"carrier_{id}";

                if (!_cache.TryGetValue(cacheKey, out CarrierModel carrier))
                {
                    carrier = await _carrierRepository.GetCarrierById(id);

                    if (carrier == null)
                    {
                        _logger.LogWarning("Carrier with ID {id} not found.", id);
                        return NotFound($"Carrier with ID {id} not found.");
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    };

                    _cache.Set(cacheKey, carrier, cacheOptions);
                }

                return Ok(carrier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving carrier with ID {carrierId}.", id);
                return StatusCode(500, "An error occurred while retrieving the carrier. Please try again later.");
            }
        }

        [HttpPost]
        [Route("AddCarrier")]
        public async Task<IActionResult> AddCarrier([FromBody] CarrierRequest model)
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

                if (model.CarrierId > 0)
                {
                    var updatedCarrier = await _carrierRepository.UpdateCarrier(model);
                    if (updatedCarrier != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"carrier_{updatedCarrier}";
                        _cache.Set(cacheKey, updatedCarrier, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Carrier updated successfully." });
                }
                else
                {
                    await _carrierRepository.AddCarrier(model);
                }

                return Ok(new { issuccess = true, message = "Carrier created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the Carrier.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while creating the Template. Please try again later." });
            }
        }

        [HttpDelete]
        [Route("DeleteCarrier/{carrierId}")]
        public async Task<IActionResult> DeleteCarrier(int carrierId)
        {
            try
            {
                if (carrierId <= 0)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid carrierId ID." });
                }

                var result = await _carrierRepository.DeleteCarrier(carrierId);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "Carrier not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "Carrier deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the Carrier.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the Carrier. Please try again later." });
            }
        }
    }
}
