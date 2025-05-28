using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using stockbridge_api.Filters;
using stockbridge_DAL.CacheHelper;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;
using stockbridge_DAL.Repositories;

namespace stockbridge_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    //[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme)]

    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ClientController> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

        public ClientController(IClientRepository clientRepository, IMemoryCache cache, ILogger<ClientController> logger)
        {
            _clientRepository = clientRepository;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves paginated list of clients.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost("GetClientsPaginated")]
        [ValidateModelState]
        public async Task<ActionResult<PaginatedResult<ClientDTO>>> GetClientsPaginatedAsync(ClientFilterRequest request)
        {
            try
            {
                string cacheKey = CacheHelper.GenerateCacheKey(request);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<ClientDTO> clients))
                {
                    clients = await _clientRepository.GetClientsPaginatedAsync(request);

                    if (clients == null || clients.Items.Count() == 0)
                    {

                    }
                    else
                    {
                        var cacheOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration,
                            SlidingExpiration = TimeSpan.FromSeconds(30)
                        };

                        _cache.Set(cacheKey, clients, cacheOptions);
                    }
                }

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated clients.");
                return StatusCode(500, "An error occurred while retrieving clients. Please try again later.");
            }
        }

        /// <summary>
        /// Retrieves a client by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("clients/{id}")]
        public async Task<ActionResult<ClientDTO>> GetClientById(int id)
        {
            try
            {
                string cacheKey = $"client_{id}";

                if (!_cache.TryGetValue(cacheKey, out ClientDTO client))
                {
                    client = await _clientRepository.GetClientByIdAsync(id);

                    if (client == null)
                    {
                        _logger.LogWarning("Client with ID {ClientId} not found.", id);
                        return NotFound($"Client with ID {id} not found.");
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    };

                    _cache.Set(cacheKey, client, cacheOptions);
                }

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving client with ID {ClientId}.", id);
                return StatusCode(500, "An error occurred while retrieving the client. Please try again later.");
            }
        }

        [HttpDelete]
        [Route("DeleteClient/{clientId}")]
        public async Task<IActionResult> DeleteClient(int clientId)
        {
            try
            {
                if (clientId <= 0)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid client ID." });
                }

                var result = await _clientRepository.DeleteClient(clientId);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "Client not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "Client deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the client.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the client. Please try again later." });
            }
        }


        [HttpPost]
        [Route("AddClient")]
        public async Task<IActionResult> AddClient([FromBody] ClientRequest model)
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

                if (model?.BasicTab?.ClientId > 0)
                {
                    var updatedClient = await _clientRepository.UpdateClientAsync(model);
                    if (updatedClient != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"client_{updatedClient.ClientId}";
                        _cache.Set(cacheKey, updatedClient, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Client updated successfully." });
                }
                else
                {
                    await _clientRepository.AddClientAsync(model);

                }

                return Ok(new { issuccess = true, message = "Client created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the client.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while creating the client. Please try again later." });
            }
        }

        [HttpPost("GetStaff")]
        [ValidateModelState]
        public async Task<ActionResult<PaginatedResult<ClientDTO>>> GetStaffAsync([FromBody] string name = null)
        {
            try
            {
                var staffList = await _clientRepository.GetStaffAsync(name);

                if (staffList == null || staffList.Items.Count() == 0)
                {
                    return Ok(new { issuccess = false, message = "No staff found." });
                }

                return Ok(new { issuccess = true, message = "Staff retrieved successfully.", list = staffList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated clients.");
                return StatusCode(500, "An error occurred while retrieving clients. Please try again later.");
            }
        }

        [HttpGet("GetStaffById/{id}")]
        public async Task<ActionResult<PaginatedResult<StaffViewModel>>> GetStaffById(int id)
        {
            try
            {
                string cacheKey = $"staff{id}";

                if (!_cache.TryGetValue(cacheKey, out StaffViewModel staff))
                {
                    staff = await _clientRepository.GetStaffByIdAsync(id);

                    if (staff == null)
                    {
                        _logger.LogWarning("Staff with ID {id} not found.", id);
                        return NotFound($"Staff with ID {id} not found.");
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    };

                    _cache.Set(cacheKey, staff, cacheOptions);
                }

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving staff with ID {StaffId}.", id);
                return StatusCode(500, "An error occurred while retrieving the staff. Please try again later.");
            }
        }

        [HttpPost("GetClientContactsPaginated")]
        [ValidateModelState]
        public async Task<ActionResult<PaginatedResult<ClientContact>>> GetClientContactsPaginatedAsync(ClientContactFilterRequest request)
        {
            try
            {
                string cacheKey = CacheHelper.GenerateCacheKey(request);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<ClientContact> clientContacts))
                {
                    clientContacts = await _clientRepository.GetClientContacts(request);

                    if (clientContacts == null || clientContacts.Items.Count() == 0)
                    {

                    }
                    else
                    {
                        var cacheOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration,
                            SlidingExpiration = TimeSpan.FromSeconds(30)
                        };

                        _cache.Set(cacheKey, clientContacts, cacheOptions);
                    }
                }

                return Ok(clientContacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated clients.");
                return StatusCode(500, "An error occurred while retrieving clients. Please try again later.");
            }
        }


        [HttpPost]
        [Route("AddStaff")]
        public async Task<IActionResult> AddStaff([FromBody] StaffRequest model)
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

                if (model?.StaffId > 0)
                {
                    var updatedStaff = await _clientRepository.UpdateStaff(model);
                    if (updatedStaff != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"staff{updatedStaff.StaffId}";
                        _cache.Set(cacheKey, updatedStaff, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Client updated successfully." });
                }
                else
                {
                    await _clientRepository.AddStaff(model);
                }

                return Ok(new { issuccess = true, message = "Staff created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the staff.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while creating the staff. Please try again later." });
            }
        }

        [HttpPost]
        [Route("AddTimeData")]
        public async Task<IActionResult> AddTimeData([FromBody] List<TimeDatumRequest> modelList)
        {
            try
            {
                if (modelList == null)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid data received." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid model data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var updatedStaff = await _clientRepository.UpdateTimeData(modelList);

                return Ok(new { issuccess = true, message = "record updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the timedata.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while updating the timedata. Please try again later." });
            }
        }

        [HttpPost("GetStaffMasterList")]
        [ValidateModelState]
        public async Task<ActionResult<PaginatedResult<ClientDTO>>> GetStaffMasterListAsync(StaffFilterRequest model)
        {
            try
            {
                string cacheKey = CacheHelper.GenerateCacheKey(model);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<Staff> staffList))
                {
                    staffList = await _clientRepository.GetStaffAsync(model.SearchQuery, model.PageNumber, model.PageSize, model.ClientID,model.IsActive,model.IsNonActive);

                    if (staffList == null || !staffList.Items.Any())
                    {
                        return Ok(new { issuccess = false, message = "No data found for the provided name." });
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    };

                    _cache.Set(cacheKey, staffList, cacheOptions);
                }

                return Ok(new { issuccess = true, message = "Staff retrieved successfully.", list = staffList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated Staff.");
                return StatusCode(500, "An error occurred while retrieving Staff. Please try again later.");
            }
        }


        [HttpPost("GetTimeDataList")]
        public async Task<ActionResult<PaginatedResult<TimeSheet>>> GetTimeDataListAsync([FromBody] GetTimeDataRequest model)
        {
            try
            {
                if ((model.StaffId == null || model.StaffId <= 0) && !string.IsNullOrEmpty(model.Email))
                {
                    var staffIdByEmail = await _clientRepository.GetStaffIdByEmail(model.Email);
                    model.StaffId = staffIdByEmail;
                }

                var timeDataList = await _clientRepository.GetTimeListData(model.StaffId ?? 0);

                if (timeDataList == null || !timeDataList.Any())
                {
                    return Ok(new { issuccess = false, message = "No data found for the provided name." });
                }

                return Ok(new { issuccess = true, message = "TimeData retrieved successfully.", list = timeDataList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving timeDataList.");
                return StatusCode(500, "An error occurred while retrieving timeDataList. Please try again later.");
            }
        }

    }
}
