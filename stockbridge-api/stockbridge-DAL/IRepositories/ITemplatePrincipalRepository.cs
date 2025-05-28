using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;

namespace stockbridge_DAL.IRepositories
{
    public interface ITemplatePrincipalRepository
    {
        /// <summary>
        /// Get Template Principal
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedResult<TemplatePrincipalModel>> GetTemplatePrincipal(string name = null, int pageNumber = 1, int pageSize = 100);
        Task<PaginatedResult<PolicyMasterListModel>> GetPolicyMasterList(string name = null, int pageNumber = 1, int pageSize = 100,bool? isActive = false, bool? isExpired = false, string sortBy = null, bool? isAscending = true);

        /// <summary>
        /// Get Brokers
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedResult<BrokerModel>> GetBrokers(string name = null, int pageNumber = 1, int pageSize = 100);

        /// <summary>
        /// Get Carriers
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedResult<CarrierModel>> GetCarriers(string name = null, int pageNumber = 1, int pageSize = 100, bool forStarting = false);

        /// <summary>
        /// Add Template Principal
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
       Task<TemplatePrincipal> AddTemplatePrincipal(TemplateRequest model);

        /// <summary>
        /// Add Policy
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
       Task<Policy> AddPolicy(PolicyRequest model);

        /// <summary>
        /// Get Policy By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<PolicyModel> GetPolicyByIdAsync(int id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TemplatePrincipalModel> GetTemplatePrincipalByIdAsync(int id);

        /// <summary>
        /// Delete Policy
        /// </summary>
        /// <param name="policyId"></param>
        /// <returns></returns>
        Task<bool> DeletePolicy(int policyId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="templatePrincipalId"></param>
        /// <returns></returns>
        Task<bool> DeleteTemplatePrincipal(int templatePrincipalId);

        /// <summary>
        /// Update Policy
        /// </summary>
        /// <param name="policyId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Policy> UpdatePolicy(PolicyRequest model);

        /// <summary>
        /// Update Template Principal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<TemplatePrincipal> UpdateTemplatePrincipal(TemplateRequest model);

        /// <summary>
        /// Get policy by client id
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PaginatedResult<PolicyMasterListModel>> GetPolicyByClientId(int clientId, int pageNumber = 1, int pageSize = 100);

        /// <summary>
        /// Update policy expiration date
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Policy</returns>
        Task<Policy> UpdatePolicyExpiration(PolicyExpirationRequest model);

        /// <summary>
        /// Delete Minors 
        /// </summary>
        /// <param name="minorId"></param>
        /// <returns></returns>
        Task<bool> DeleteMinor(List<int> minorId);

        /// <summary>
        /// Delete Minor Row
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> DeleteMinorRowByRowSequence(DeleteMinorRequest request);

        /// <summary>
        /// Update policy print sequence
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdatePolicyPrintSequence(UpdatePolicyPrientSequenceRequest model);
    }
}
