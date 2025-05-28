using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using stockbridge_api.Filters;
using stockbridge_DAL.CacheHelper;
using stockbridge_DAL.DTOs;
using stockbridge_DAL.IRepositories;
using stockbridge_DAL.Repositories;

namespace stockbridge_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        public readonly ITemplatePrincipalRepository _templatePrincipalRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PolicyController> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);
        private readonly PrintPolicy _printPolicy;

        public PolicyController(ITemplatePrincipalRepository templatePrincipalRepository,
            IMemoryCache cache,
            ILogger<PolicyController> logger,
            PrintPolicy printPolicy)
        {
            _templatePrincipalRepository = templatePrincipalRepository;
            _cache = cache;
            _logger = logger;
            _printPolicy = printPolicy;
        }

        [HttpPost("GetTemplatePrincipal")]
        [ValidateModelState]
        public async Task<ActionResult<PaginatedResult<TemplatePrincipalModel>>> GetTemplatePrincipalAsync(TemplatePrincipalFilterRequest request)
        {
            try
            {
                string cacheKey = CacheHelper.GenerateCacheKey(request);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<TemplatePrincipalModel> templatePrincipal))
                {
                    templatePrincipal = await _templatePrincipalRepository.GetTemplatePrincipal(request.SearchName, request.PageNumber, request.PageSize);

                    if (templatePrincipal == null || !templatePrincipal.Items.Any())
                    {
                        return Ok(new { issuccess = false, message = "No data found for the provided name." });
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    };

                    _cache.Set(cacheKey, templatePrincipal, cacheOptions);
                }

                return Ok(new { issuccess = true, message = "Staff retrieved successfully.", list = templatePrincipal });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated templatePrincipal for name: {Name}, Page: {PageNumber}, Size: {PageSize}", request.SearchName, request.PageNumber, request.PageSize);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        [HttpPost("GetPolicyMasterList")]
        [ValidateModelState]
        public async Task<ActionResult<PaginatedResult<PolicyMasterListModel>>> GetPolicyMasterListAsync(TemplatePrincipalFilterRequest request)
        {
            try
            {
                string cacheKey = CacheHelper.GenerateCacheKey(request);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<PolicyMasterListModel> templatePrincipal))
                {
                    templatePrincipal = await _templatePrincipalRepository.GetPolicyMasterList(request.SearchName, request.PageNumber, request.PageSize, request.IsActive, request.IsExpired, request.SortBy, request.IsAscending);

                    if (templatePrincipal == null || !templatePrincipal.Items.Any())
                    {
                        return Ok(new { issuccess = false, message = "No data found for the provided name." });
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    };

                    _cache.Set(cacheKey, templatePrincipal, cacheOptions);
                }

                return Ok(new { issuccess = true, message = "Staff retrieved successfully.", list = templatePrincipal });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated templatePrincipal for name: {Name}, Page: {PageNumber}, Size: {PageSize}", request.SearchName, request.PageNumber, request.PageSize);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }


        [HttpPost]
        [Route("GetBroker")]
        public async Task<ActionResult<PaginatedResult<BrokerModel>>> GetBroker(BrokerFilterRequest model)
        {
            try
            {

                string cacheKey = CacheHelper.GenerateCacheKey(model);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<BrokerModel> broker))
                {
                    broker = await _templatePrincipalRepository.GetBrokers(model.SearchQuery, model.PageNumber, model.PageSize);

                    if (broker == null || !broker.Items.Any())
                    {
                        return Ok(new { issuccess = false, message = "No data found for the provided name." });
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    };

                    _cache.Set(cacheKey, broker, cacheOptions);
                }

                return Ok(new { issuccess = true, message = "Broker retrieved successfully.", list = broker });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated broker for name: {Name}, Page: {PageNumber}, Size: {PageSize}", model.SearchQuery, model.PageNumber, model.PageSize);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        [HttpPost]
        [Route("GetCarrier")]
        public async Task<ActionResult<PaginatedResult<CarrierModel>>> GetCarrier(CarrierFilterRequest model)
        {
            try
            {

                string cacheKey = CacheHelper.GenerateCacheKey(model);

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<CarrierModel> carrier))
                {
                    carrier = await _templatePrincipalRepository.GetCarriers(model.SearchQuery, model.PageNumber, model.PageSize, model.ForStarting);

                    if (carrier == null || !carrier.Items.Any())
                    {
                        return Ok(new { issuccess = false, message = "No data found for the provided name." });
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    };

                    _cache.Set(cacheKey, carrier, cacheOptions);
                }

                return Ok(new { issuccess = true, message = "Carrier retrieved successfully.", list = carrier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving paginated carrier for name: {Name}, Page: {PageNumber}, Size: {PageSize}", model.SearchQuery, model.PageNumber, model.PageSize);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        [HttpPost]
        [Route("AddTemplate")]
        public async Task<IActionResult> AddTemplatePrincipal([FromBody] TemplateRequest model)
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

                if (model?.TemplatePrincipal?.PrincipalId != null && model?.TemplatePrincipal?.PrincipalId > 0)
                {
                    var updatedTemplate = await _templatePrincipalRepository.UpdateTemplatePrincipal(model);
                    if (updatedTemplate != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"temPolicy_{updatedTemplate.PrincipalId}";
                        _cache.Set(cacheKey, updatedTemplate, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Template updated successfully." });
                }
                else
                {
                    await _templatePrincipalRepository.AddTemplatePrincipal(model);
                }

                return Ok(new { issuccess = true, message = "Template created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the Template.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while creating the Template. Please try again later." });
            }
        }

        [HttpPost]
        [Route("AddPolicy")]
        public async Task<IActionResult> AddPolicy([FromBody] PolicyRequest model)
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

                if (model?.PolicyId > 0)
                {
                    var updatedPolicy = await _templatePrincipalRepository.UpdatePolicy(model);
                    if (updatedPolicy != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"policy_{updatedPolicy.PolicyId}";
                        _cache.Set(cacheKey, updatedPolicy, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Policy updated successfully.", PolicyId = updatedPolicy.PolicyId });
                }

                var policy = await _templatePrincipalRepository.AddPolicy(model);

                return Ok(new { issuccess = true, message = "policy created successfully.", PolicyId = policy.PolicyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the Policy.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while creating the Policy. Please try again later." });
            }
        }


        [HttpGet("GetPolicyById/{id}")]
        public async Task<ActionResult<PaginatedResult<PolicyModel>>> GetPolicyById(int id)
        {
            try
            {
                //string cacheKey = $"policy_{id}";

                //if (!_cache.TryGetValue(cacheKey, out PolicyModel policy))
                //{
                var policy = await _templatePrincipalRepository.GetPolicyByIdAsync(id);

                if (policy == null)
                {
                    _logger.LogWarning("Policy with ID {id} not found.", id);
                    return NotFound($"Policy with ID {id} not found.");
                }

                //var cacheOptions = new MemoryCacheEntryOptions
                //{
                //    AbsoluteExpirationRelativeToNow = _cacheDuration
                //};

                //_cache.Set(cacheKey, policy, cacheOptions);
                //}

                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving policy with ID {PolicyId}.", id);
                return StatusCode(500, "An error occurred while retrieving the policy. Please try again later.");
            }
        }

        [HttpPost("PrintPolicy/{policyId}")]
        public async Task<ActionResult> PrintPolicy(int policyId)
        {
            try
            {
                byte[] fileBytes = await _printPolicy.GenerateWordDocumentAsync(policyId);
                var fileName = $"Policy_{policyId}_{DateTime.Now:yyyyMMdd}.docx";

                //return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "PolicyDetail.docx");
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetTemplatePrincipalById/{id}")]
        public async Task<ActionResult<PaginatedResult<TemplatePrincipalModel>>> GetTemplatePrincipalById(int id)
        {
            try
            {
                string cacheKey = $"temPolicy_{id}";

                if (!_cache.TryGetValue(cacheKey, out TemplatePrincipalModel templatePrincipal))
                {
                    templatePrincipal = await _templatePrincipalRepository.GetTemplatePrincipalByIdAsync(id);

                    if (templatePrincipal == null)
                    {
                        _logger.LogWarning("TemplatePrincipal with ID {id} not found.", id);
                        return NotFound($"TemplatePrincipal with ID {id} not found.");
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    };

                    _cache.Set(cacheKey, templatePrincipal, cacheOptions);
                }

                return Ok(templatePrincipal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving TemplatePrincipal with ID {PrincipalId}.", id);
                return StatusCode(500, "An error occurred while retrieving the TemplatePrincipal. Please try again later.");
            }
        }

        [HttpDelete]
        [Route("DeleteTemplatePrincipal/{templatePrincipalId}")]
        public async Task<IActionResult> DeleteTemplatePrincipal(int templatePrincipalId)
        {
            try
            {
                if (templatePrincipalId <= 0)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid templatePrincipalId ID." });
                }

                var result = await _templatePrincipalRepository.DeleteTemplatePrincipal(templatePrincipalId);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "TemplatePrincipal not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "TemplatePrincipal deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the TemplatePrincipal.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the TemplatePrincipal. Please try again later." });
            }
        }

        [HttpDelete]
        [Route("DeletePolicy/{policyId}")]
        public async Task<IActionResult> DeletePolicy(int policyId)
        {
            try
            {
                if (policyId <= 0)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid policy ID." });
                }

                var result = await _templatePrincipalRepository.DeletePolicy(policyId);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "Policy not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "Policy deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the policy.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the policy. Please try again later." });
            }
        }

        [HttpGet("GetPolicyByClientId/{clientId}")]
        public async Task<ActionResult<PaginatedResult<PolicyMasterListModel>>> GetPolicyByClientId(int clientId)
        {
            try
            {
                string cacheKey = $"clientId_{clientId}";

                if (!_cache.TryGetValue(cacheKey, out PaginatedResult<PolicyMasterListModel> templatePrincipal))
                {
                    templatePrincipal = await _templatePrincipalRepository.GetPolicyByClientId(clientId, 1, 200);

                    if (templatePrincipal == null || !templatePrincipal.Items.Any())
                    {
                        return Ok(new { issuccess = false, message = "No data found for the provided name." });
                    }

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheDuration,
                        SlidingExpiration = TimeSpan.FromSeconds(30)
                    };

                    _cache.Set(cacheKey, templatePrincipal, cacheOptions);
                }

                return Ok(new { issuccess = true, message = "Policy retrieved successfully.", list = templatePrincipal });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving  templatePrincipal. by  {clientId}", clientId);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        [HttpPost]
        [Route("UpdatePolicyExpirationDate")]
        public async Task<IActionResult> UpdatePolicyExpirationDate([FromBody] PolicyExpirationRequest model)
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

                if (model?.PolicyId > 0)
                {
                    var updatedPolicy = await _templatePrincipalRepository.UpdatePolicyExpiration(model);
                    if (updatedPolicy != null)
                    {
                        // Invalidate or update cache
                        string cacheKey = $"policy_{updatedPolicy.PolicyId}";
                        _cache.Set(cacheKey, updatedPolicy, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = _cacheDuration
                        });

                    }
                    return Ok(new { issuccess = true, message = "Policy updated successfully." });
                }

                return Ok(new { issuccess = false, message = "Error occurred while updating policy expiration date." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating policy expiration date.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while updating policy expiration date. Please try again later." });
            }
        }

        [HttpPost]
        [Route("DeleteMinors")]
        public async Task<IActionResult> DeleteMinors([FromBody] List<int> minorIds)
        {
            try
            {
                if (minorIds == null || !minorIds.Any())
                {
                    return BadRequest(new { issuccess = false, message = "No Minor IDs provided." });
                }

                var result = await _templatePrincipalRepository.DeleteMinor(minorIds);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "Some or all minors not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "Minors deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the Minors.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the Minors. Please try again later." });
            }
        }

        [HttpPost]
        [Route("DeleteMinorRows")]
        public async Task<IActionResult> DeleteMinorRows([FromBody] DeleteMinorRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { issuccess = false, message = "No Minor IDs provided." });
                }

                var result = await _templatePrincipalRepository.DeleteMinorRowByRowSequence(request);

                if (!result)
                {
                    return NotFound(new { issuccess = false, message = "Some or all minors not found or could not be deleted." });
                }

                return Ok(new { issuccess = true, message = "Minors deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the Minors.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while deleting the Minors. Please try again later." });
            }
        }

        [HttpPost]
        [Route("UpdatePolicyPrientSequence")]
        public async Task<IActionResult> UpdatePolicyPrientSequence([FromBody] UpdatePolicyPrientSequenceRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid data received." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { issuccess = false, message = "Invalid model data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                if (request?.ClientId > 0)
                {
                    await _templatePrincipalRepository.UpdatePolicyPrintSequence(request);

                    string cacheKey = $"clientId_{request.ClientId}";
                    _cache.Remove(cacheKey);

                    return Ok(new { issuccess = true, message = "Policy sequence updated successfully." });
                }

                return Ok(new { issuccess = false, message = "Error occurred while updating policy sequence." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating policy expiration date.");
                return StatusCode(500, new { issuccess = false, message = "An error occurred while updating policy expiration date. Please try again later." });
            }
        }

    }
}
