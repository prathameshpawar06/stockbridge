using AutoMapper;
using DocumentFormat.OpenXml.Vml;
using Microsoft.EntityFrameworkCore;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;
using stockbridge_DAL.IRepositories;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using stockbridge_DAL.domainModels;
using Azure.Core;

namespace stockbridge_DAL.Repositories
{
    public class TemplatePrincipalRepository : ITemplatePrincipalRepository
    {
        private readonly StockbridgeContext _context;
        private readonly IMapper _mapper;

        public TemplatePrincipalRepository(StockbridgeContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Get Template Principal
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<TemplatePrincipalModel>> GetTemplatePrincipal(string name = null, int pageNumber = 1, int pageSize = 100)
        {
            //var query = _context.TemplatePrincipals
            //            .AsQueryable();

            var query = _context.TemplatePrincipals
            .Select(x => new TemplatePrincipal
            {
                PrincipalId = x.PrincipalId,
                Name = x.Name,
                Description = x.Description,
                TimeStamp = x.TimeStamp
            })
            .AsQueryable();


            //Apply Filters 

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            var totalItems = await query.CountAsync();

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var listModel = _mapper.Map<List<TemplatePrincipalModel>>(items);

            return new PaginatedResult<TemplatePrincipalModel>
            {
                Items = listModel,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<PolicyMasterListModel>> GetPolicyMasterList(string name = null, int pageNumber = 1, int pageSize = 100,bool? isActive = false,bool? isExpired = false, string sortBy = null, bool? isAscending = true)
        {
            var policyTypeLists = await _context.PolicyTypes.ToListAsync();

            var policyList = _context.Policies.AsQueryable();
            if (isActive == true)
            {
                policyList = policyList.Where(x => x.Expired == false);
            }

            if(isExpired == true)
            {
                policyList = policyList.Where(x => x.Expired == true);
            }

            // Sort by logic
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "policyno":
                        policyList = isAscending == true
                            ? policyList.OrderBy(x => x.PolicyNo)
                            : policyList.OrderByDescending(x => x.PolicyNo);
                        break;

                    case "policytitle":
                        policyList = isAscending == true
                            ? policyList.OrderBy(x => x.PolicyTitle)
                            : policyList.OrderByDescending(x => x.PolicyTitle);
                        break;

                    default:
                        policyList = isAscending == true
                            ? policyList.OrderBy(x => x.PolicyNo)
                            : policyList.OrderByDescending(x => x.PolicyNo);
                        break;
                }
            }

            var query = policyList
            .Select(x => new PolicyMasterListModel
            {
                PolicyId = x.PolicyId,
                PolicyNo = x.PolicyNo,
                PolicyTitle = x.PolicyTitle,
                PolicyType = x.PolicyType,
                ExpirationDate = x.ExpirationDate,
                Client = x.Client,
                Carrier = x.Carrier,
                Principal = x.Principal
            })
            .AsQueryable();


            //Apply Filters 

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.PolicyTitle.Contains(name) || x.PolicyNo.Contains(name) || x.Client.CompanyName.Contains(name));
            }

            var totalItems = await query.CountAsync();

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var listModel = _mapper.Map<List<PolicyMasterListModel>>(items);

            foreach (var item in listModel)
            {
                item.PolicyTypeName = policyTypeLists.Where(y => y.PolicyTypeId == item.PolicyType).Select(x => x.PolicyTypeName).FirstOrDefault();

            }

            return new PaginatedResult<PolicyMasterListModel>
            {
                Items = listModel,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

        }

        /// <summary>
        /// Get Brokers
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<BrokerModel>> GetBrokers(string name = null, int pageNumber = 1, int pageSize = 100)
        {
            var query = _context.Brokers
            .Select(x => new BrokerModel
            {
                BrokerId = x.BrokerId,
                Name = x.Name,
                Address1 = x.Address1,
                Address2 = x.Address2,
                Address3 = x.Address3,
                City = x.City,
                State = x.State,
                Zip = x.Zip,
                Telephone = x.Telephone,
                Fax = x.Fax,
                TimeStamp = x.TimeStamp
            })
            .AsQueryable();


            //Apply Filters 

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            var totalItems = await query.CountAsync();

            if(pageSize != -1)
            {
                query =  query.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize);
            }
            
            return new PaginatedResult<BrokerModel>
            {
                Items = await query.ToListAsync(),
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

        }

        /// <summary>
        /// Get Carriers
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<CarrierModel>> GetCarriers(string name = null, int pageNumber = 1, int pageSize = 100, bool forStarting = false)
        {
            var query = _context.Carriers
            .Select(x => new CarrierModel
            {
                CarrierId = x.CarrierId,
                Name = x.Name,
                Description = x.Description,
                AmBest = x.AmBest,
                Licensed = x.Licensed,
                TimeStamp = x.TimeStamp
            })
            .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(name))
            {
                query = forStarting
                    ? query.Where(x => x.Name.StartsWith(name))
                    : query.Where(x => x.Name.Contains(name));
            }

            var totalItems = await query.CountAsync();

            if(pageSize != -1)
            {
                query = query.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize);
            }


            return new PaginatedResult<CarrierModel>
            {
                Items = await query.ToListAsync(),
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

        }

        /// <summary>
        /// Add Template Principal
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TemplatePrincipal> AddTemplatePrincipal(TemplateRequest model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                var template = _mapper.Map<TemplatePrincipal>(model.TemplatePrincipal);
                _context.TemplatePrincipals.Add(template);
                await _context.SaveChangesAsync();

                foreach (var templateMajor in model.TemplateMajor)
                {
                    var templateMajorData = _mapper.Map<TemplateMajor>(templateMajor);

                    templateMajorData.PrincipalId = template.PrincipalId;
                    _context.TemplateMajors.Add(templateMajorData);
                    await _context.SaveChangesAsync();

                    foreach (var templateMajorColDef in templateMajor.TemplateMajorColDef)
                    {
                        var templateMajorColDefData = _mapper.Map<TemplateMajorColDef>(templateMajorColDef);

                        templateMajorColDefData.MajorId = templateMajorData.MajorId;
                        templateMajorColDefData.ColumnType = templateMajorColDef.ColumnType;
                        _context.TemplateMajorColDefs.Add(templateMajorColDefData);
                        await _context.SaveChangesAsync();

                        //foreach (var reqTemplateMinorDef in templateMajorColDef?.TemplateMinorDefs)
                        //{
                        //    var reqTemplateMinorDefData = _mapper.Map<TemplateMinorDef>(reqTemplateMinorDef);
                        //    reqTemplateMinorDefData.ColumnDefId = templateMajorColDefData.ColumnDefId;
                        //    _context.TemplateMinorDefs.Add(reqTemplateMinorDefData);
                        //}

                    }

                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return template;

            }
            catch (Exception ex)
            {
                // Rollback in case of an error
                await transaction.RollbackAsync();
                throw ex;  // Rethrow the exception to be handled higher up
            }
        }

        ///// <summary>
        ///// Update Template Principal
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public async Task<TemplatePrincipal> UpdateTemplatePrincipal( TemplateRequest model)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var existingTemplate = await _context.TemplatePrincipals
        //            .Include(tp => tp.TemplateMajors)
        //                .ThenInclude(tm => tm.TemplateMajorColDefs)
        //                .ThenInclude(tc => tc.TemplateMinorDefs)
        //            .FirstOrDefaultAsync(tp => tp.PrincipalId == model.TemplatePrincipal.PrincipalId);

        //        if (existingTemplate == null)
        //            throw new Exception("Template Principal not found");

        //        // Update main entity
        //        _mapper.Map(model.TemplatePrincipal, existingTemplate);

        //        // Handle TemplateMajors
        //        foreach (var major in existingTemplate.TemplateMajors.ToList())
        //        {
        //            if (!model.TemplateMajor.Any(m => m.MajorId == major.MajorId))
        //            {
        //                existingTemplate.TemplateMajors.Remove(major);
        //            }
        //        }

        //        foreach (var newMajor in model.TemplateMajor)
        //        {
        //            var existingMajor = existingTemplate.TemplateMajors.FirstOrDefault(m => m.MajorId == newMajor.MajorId);
        //            if (existingMajor == null)
        //            {
        //                var newMajorData = _mapper.Map<TemplateMajor>(newMajor);
        //                newMajorData.PrincipalId = existingTemplate.PrincipalId;
        //                existingTemplate.TemplateMajors.Add(newMajorData);
        //            }
        //            else
        //            {
        //                _mapper.Map(newMajor, existingMajor);

        //                // Handle TemplateMajorColDefs
        //                foreach (var colDef in existingMajor.TemplateMajorColDefs.ToList())
        //                {
        //                    if (!newMajor.TemplateMajorColDef.Any(cd => cd.ColumnDefId == colDef.ColumnDefId))
        //                    {
        //                        existingMajor.TemplateMajorColDefs.Remove(colDef);
        //                    }
        //                }

        //                foreach (var newColDef in newMajor.TemplateMajorColDef)
        //                {
        //                    var existingColDef = existingMajor.TemplateMajorColDefs.FirstOrDefault(cd => cd.ColumnDefId == newColDef.ColumnDefId);
        //                    if (existingColDef == null)
        //                    {
        //                        var newColDefData = _mapper.Map<TemplateMajorColDef>(newColDef);
        //                        newColDefData.MajorId = existingMajor.MajorId;
        //                        existingMajor.TemplateMajorColDefs.Add(newColDefData);
        //                    }
        //                    else
        //                    {
        //                        _mapper.Map(newColDef, existingColDef);

        //                        // Handle TemplateMinorDefs
        //                        foreach (var minorDef in existingColDef.TemplateMinorDefs.ToList())
        //                        {
        //                            if (!newColDef.ReqTemplateMinorDef.Any(md => md.MinorId == minorDef.MinorId))
        //                            {
        //                                existingColDef.TemplateMinorDefs.Remove(minorDef);
        //                            }
        //                        }

        //                        foreach (var newMinorDef in newColDef.ReqTemplateMinorDef)
        //                        {
        //                            var existingMinorDef = existingColDef.TemplateMinorDefs.FirstOrDefault(md => md.MinorId == newMinorDef.MinorId);
        //                            if (existingMinorDef == null)
        //                            {
        //                                var newMinorDefData = _mapper.Map<TemplateMinorDef>(newMinorDef);
        //                                newMinorDefData.ColumnDefId = existingColDef.ColumnDefId;
        //                                existingColDef.TemplateMinorDefs.Add(newMinorDefData);
        //                            }
        //                            else
        //                            {
        //                                _mapper.Map(newMinorDef, existingMinorDef);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return existingTemplate;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        throw new Exception($"Error updating Template Principal: {ex.Message}");
        //    }
        //}

        /// <summary>
        /// Add Policy
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Policy> AddPolicy(PolicyRequest model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the latest PrintSequence for the client 
                var latestPolicy = await _context.Policies
                    .Where(x => x.ClientId == model.ClientId)
                    .OrderByDescending(x => x.PrintSequence)
                    .FirstOrDefaultAsync();
                var printSequence = latestPolicy?.PrintSequence ?? 0;

                var contactStaff = await _context.ClientStaffs.FirstOrDefaultAsync(x => x.ClientId == model.ClientId);
                var policy = _mapper.Map<Policy>(model);
                //policy.ExpirationDate = policy.ExpirationDate.HasValue && policy.ExpirationDate >= new DateTime(1753, 1, 1) ? policy.ExpirationDate : null;
                //policy.InceptionDate = policy.InceptionDate.HasValue && policy.InceptionDate >= new DateTime(1753, 1, 1) ? policy.InceptionDate : null;
                policy.AddDate = DateTime.UtcNow;
                policy.ChangeDate = DateTime.Now;
                policy.AddUid = "sa";
                policy.ChangeUid = "sa";
                policy.Status = "";
                policy.StaffId = contactStaff != null ?  contactStaff.StaffId : 0;
                policy.PolicyType = 1;
                policy.ParentPolicy = 2;
                policy.PrintSequence = printSequence + 1;

                _context.Policies.Add(policy);
                await _context.SaveChangesAsync();

                foreach (var entity in model.SelectedEntityId)
                {
                    var policyEntity = new PolicyEntity
                    {
                        PolicyId = policy.PolicyId,
                        ClientId = model.ClientId,
                        EntityId = entity,
                        AddDate = DateTime.Now,
                        ChangeDate = DateTime.Now,
                        ChangeUid = "sa",
                        AddUid = "Sa"
                    };

                    _context.PolicyEntities.Add(policyEntity);
                    await _context.SaveChangesAsync();
                }

                foreach (var location in model.SelectedLocationId)
                {
                    var policyLocation = new PolicyLocation
                    {
                        PolicyId = policy.PolicyId,
                        ClientId = model.ClientId,
                        LocationId = location,
                        AddDate = DateTime.Now,
                        ChangeDate = DateTime.Now,
                        ChangeUid = "sa",
                        AddUid = "Sa"
                    };

                    _context.PolicyLocations.Add(policyLocation);
                }

                var templatePrincipal = await _context.TemplatePrincipals
                   .AsSplitQuery()
                   .Include(x => x.TemplateMajors)
                   .ThenInclude(x => x.TemplateMajorColDefs)
                   .ThenInclude(x => x.TemplateMinorDefs)
                   .FirstOrDefaultAsync(x => x.PrincipalId == model.PrincipalId);

                //For major minor 
                foreach (var templateMajor in templatePrincipal.TemplateMajors)
                {
                    //var policyMajorData = _mapper.Map<PolicyMajor>(templateMajor);
                    var policyMajorData = new PolicyMajor
                    {
                        PolicyId = policy.PolicyId,
                        Name = templateMajor.Name,
                        Comments = templateMajor.Comments,
                        Sequence = templateMajor.Sequence
                    };
                    _context.PolicyMajors.Add(policyMajorData);
                    await _context.SaveChangesAsync();

                    foreach (var templateMajorColDef in templateMajor.TemplateMajorColDefs)
                    {
                        //var policyMajorColDefData = _mapper.Map<PolicyMajorColDef>(templateMajorColDef);
                        var policyMajorColDefData = new PolicyMajorColDef
                        {
                            MajorId = policyMajorData.MajorId,
                            Sequence = templateMajorColDef.Sequence,
                            ColumnName = templateMajorColDef.ColumnName,
                            ColumnDescription = templateMajorColDef.ColumnDescription,
                            ColumnType = templateMajorColDef.ColumnType,

                        };
                        _context.PolicyMajorColDefs.Add(policyMajorColDefData);
                        await _context.SaveChangesAsync();

                        foreach (var templateMinorDef in templateMajorColDef?.TemplateMinorDefs)
                        {
                            //var policyMinorDefData = _mapper.Map<PolicyMinorDef>(templateMinorDef);
                            var policyMinorDefData = new PolicyMinorDef
                            {
                                ColumnDefId = policyMajorColDefData.ColumnDefId,
                                RowSequence = templateMinorDef.RowSequence,
                                ColumnValue = templateMinorDef.ColumnValue,
                            };
                            _context.PolicyMinorDefs.Add(policyMinorDefData);
                        }

                    }

                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return policy;

            }
            catch (Exception ex)
            {
                // Rollback in case of an error
                await transaction.RollbackAsync();
                throw ex;  // Rethrow the exception to be handled higher up
            }
        }

        /// <summary>
        /// Update policy
        /// </summary>
        /// <param name="policyId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //public async Task<Policy> UpdatePolicy(PolicyRequest model)
        //{
        //    if (model.PolicyId == null)
        //    {
        //        throw new ArgumentException("Invalid Policy ID.");
        //    }

        //    int policyId = model.PolicyId.Value;

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var policy = await _context.Policies.FindAsync(policyId);
        //        if (policy == null)
        //        {
        //            throw new Exception("Policy not found.");
        //        }

        //        var existingPolicyNo = policy.PolicyNo;

        //        // Map updated fields (Uncomment if AutoMapper is used)
        //        //_mapper.Map(model, policy);

        //        // Update policy details
        //        policy.ChangeDate = DateTime.UtcNow;
        //        policy.ChangeUid = "sa";
        //        policy.PolicyNo = existingPolicyNo; // Preserving existing PolicyNo
        //        //policy.ClientId = 3;
        //        //policy.StaffId = 18;

        //        _context.Policies.Update(policy);
        //        await _context.SaveChangesAsync();

        //        // Remove existing related entities
        //        await _context.PolicyEntities.Where(pe => pe.PolicyId == policyId).ExecuteDeleteAsync();
        //        await _context.PolicyLocations.Where(pl => pl.PolicyId == policyId).ExecuteDeleteAsync();

        //        // Add new related entities
        //        if (model.SelectedEntityId?.Any() == true)
        //        {
        //            var newPolicyEntities = model.SelectedEntityId.Select(entityId => new PolicyEntity
        //            {
        //                PolicyId = policyId,
        //                ClientId = model.ClientId,
        //                EntityId = entityId,
        //                AddDate = DateTime.UtcNow,
        //                ChangeDate = DateTime.UtcNow,
        //                ChangeUid = "sa",
        //                AddUid = "sa"
        //            }).ToList();

        //            await _context.PolicyEntities.AddRangeAsync(newPolicyEntities);
        //        }

        //        if (model.SelectedLocationId?.Any() == true)
        //        {
        //            var newPolicyLocations = model.SelectedLocationId.Select(locationId => new PolicyLocation
        //            {
        //                PolicyId = policyId,
        //                ClientId = model.ClientId,
        //                LocationId = locationId,
        //                AddDate = DateTime.UtcNow,
        //                ChangeDate = DateTime.UtcNow,
        //                ChangeUid = "sa",
        //                AddUid = "sa"
        //            }).ToList();

        //            await _context.PolicyLocations.AddRangeAsync(newPolicyLocations);
        //        }

        //        // Handle PolicyMajors
        //        if (model.PolicyMajors?.Any() == true)
        //        {
        //            var existMajorsList = await _context.PolicyMajors.Where(x => x.PolicyId == policyId).ToListAsync();
        //            var modelMajorIds = model.PolicyMajors.Select(m => m.MajorId).ToList();

        //            foreach (var major in model.PolicyMajors)
        //            {
        //                var majorData = existMajorsList.FirstOrDefault(x => x.MajorId == major.MajorId);
        //                if (majorData == null)
        //                {
        //                    var newMajor = new PolicyMajor
        //                    {
        //                        PolicyId = policyId,
        //                        Name = major.Name,
        //                        Sequence = major.Sequence,
        //                        Comments = major.Comments
        //                    };
        //                    _context.PolicyMajors.Add(newMajor);
        //                    existMajorsList.Add(newMajor); // Add to list for future reference
        //                }
        //                else
        //                {
        //                    majorData.Name = major.Name;
        //                    majorData.Sequence = major.Sequence;
        //                    majorData.Comments = major.Comments;
        //                }

        //                // Handle PolicyMajorColDefs
        //                if (major.PolicyMajorColDefs != null && major.PolicyMajorColDefs.Count() > 1)
        //                {
        //                    var existColDefList = await _context.PolicyMajorColDefs
        //                        .Where(x => x.MajorId == major.MajorId)
        //                        .ToListAsync();

        //                    var modelColDefIds = major.PolicyMajorColDefs.Select(m => m.ColumnDefId).ToList();

        //                    foreach (var policyMajor in major.PolicyMajorColDefs)
        //                    {
        //                        var existColDef = existColDefList.FirstOrDefault(x => x.ColumnDefId == policyMajor.ColumnDefId);
        //                        if (existColDef == null)
        //                        {
        //                            var newColDef = new PolicyMajorColDef
        //                            {
        //                                MajorId = major.MajorId,
        //                                ColumnName = policyMajor.ColumnName,
        //                                Sequence = policyMajor.Sequence
        //                            };
        //                            _context.PolicyMajorColDefs.Add(newColDef);
        //                        }
        //                        else
        //                        {
        //                            existColDef.ColumnName = policyMajor.ColumnName;
        //                            existColDef.Sequence = policyMajor.Sequence;
        //                        }

        //                        // Handle PolicyMinorDefs
        //                        if (policyMajor.PolicyMinorDefs != null)
        //                        {
        //                            var existPolicyMinorDef = await _context.PolicyMinorDefs
        //                                .Where(x => x.ColumnDefId == policyMajor.ColumnDefId)
        //                                .ToListAsync();

        //                            var modelPolicyMinorDataIds = policyMajor.PolicyMinorDefs.Select(x => x.MinorId).ToList();

        //                            foreach (var policyMinorDef in policyMajor.PolicyMinorDefs)
        //                            {
        //                                var policyMinorData = existPolicyMinorDef.FirstOrDefault(x => x.MinorId == policyMinorDef.MinorId);
        //                                if (policyMinorData == null)
        //                                {
        //                                    var newPolicyMinorData = new PolicyMinorDef
        //                                    {
        //                                        ColumnDefId = policyMajor.ColumnDefId,
        //                                        RowSequence = policyMinorDef.RowSequence,
        //                                        ColumnValue = policyMinorDef.ColumnValue
        //                                    };
        //                                    _context.PolicyMinorDefs.Add(newPolicyMinorData);
        //                                }
        //                                else
        //                                {
        //                                    policyMinorData.RowSequence = policyMinorDef.RowSequence;
        //                                    policyMinorData.ColumnValue = policyMinorDef.ColumnValue;
        //                                }
        //                            }

        //                            // Delete missing PolicyMinorDefs
        //                            var policyMinorToDeleted = existPolicyMinorDef
        //                                .Where(x => !modelPolicyMinorDataIds.Contains(x.MinorId))
        //                                .ToList();
        //                            if (policyMinorToDeleted.Any())
        //                            {
        //                                //_context.PolicyMinorDefs.RemoveRange(policyMinorToDeleted);
        //                            }
        //                        }
        //                    }

        //                    // Delete missing PolicyMajorColDefs
        //                    var policyMajorColDefToDeleted = existColDefList
        //                        .Where(m => !modelColDefIds.Contains(m.ColumnDefId))
        //                        .ToList();
        //                    if (policyMajorColDefToDeleted.Any())
        //                    {
        //                        //foreach (var item in policyMajorColDefToDeleted)
        //                        //{
        //                        //    var policyMinorUnderPolicyMajorColDef = await _context.PolicyMinorDefs.Where(x => x.ColumnDefId == item.ColumnDefId).ToListAsync();
        //                        //    _context.PolicyMinorDefs.RemoveRange(policyMinorUnderPolicyMajorColDef);
        //                        //}

        //                        var minorDefToDelete = await _context.PolicyMinorDefs
        //                            .Where(md => policyMajorColDefToDeleted.Select(cd => cd.ColumnDefId).Contains(md.ColumnDefId))
        //                            .ToListAsync();

        //                        _context.PolicyMinorDefs.RemoveRange(minorDefToDelete);
        //                        _context.PolicyMajorColDefs.RemoveRange(policyMajorColDefToDeleted);
        //                    }
        //                }
        //            }

        //            // Delete missing PolicyMajors
        //            var majorsToDelete = existMajorsList.Where(m => !modelMajorIds.Contains(m.MajorId)).ToList();
        //            if (majorsToDelete.Any())
        //            {
        //                //foreach (var major in majorsToDelete)
        //                //{
        //                //    var colDefToDeleteByMajor = await _context.PolicyMajorColDefs.Where(x => x.MajorId == major.MajorId).ToListAsync();
        //                //    _context.PolicyMajorColDefs.RemoveRange(colDefToDeleteByMajor);
        //                //}

        //                var colDefToDelete = await _context.PolicyMajorColDefs
        //                    .Where(cd => majorsToDelete.Select(m => m.MajorId).Contains(cd.MajorId))
        //                    .ToListAsync();

        //                //_context.PolicyMajorColDefs.RemoveRange(colDefToDelete);
        //                //_context.PolicyMajors.RemoveRange(majorsToDelete);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return policy;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw; // Preserve original stack trace
        //    }
        //}

        public async Task<Policy> UpdatePolicy(PolicyRequest model)
        {
            if (model.PolicyId == null)
            {
                throw new ArgumentException("Invalid Policy ID.");
            }

            int policyId = model.PolicyId.Value;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var policy = await _context.Policies.FindAsync(policyId);
                if (policy == null)
                {
                    throw new KeyNotFoundException("Policy not found.");
                }

                var existingPolicyNo = policy.PolicyNo;

                // Map updated fields (Uncomment if AutoMapper is used)
                //_mapper.Map(model, policy);

                // Update policy details while preserving existing PolicyNo
                policy.ChangeDate = DateTime.UtcNow;
                policy.ChangeUid = "sa";
                policy.PolicyNo = existingPolicyNo;

                policy.PolicyTitle = model.PolicyTitle;
                policy.CarrierId = model.CarrierId ?? 0;
                policy.BrokerId = model.BrokerId;
                policy.PolicyComment = model.PolicyComment;
                policy.ClientId = model.ClientId;
                policy.PrincipalId = model.PrincipalId;
                policy.ExpirationDate = model.ExpirationDate;
                policy.InceptionDate = model.InceptionDate;
                policy.StaffId = model.StaffId ?? 0;
                policy.Audit = model.Audit;
                policy.Expired = model.Expired;
                policy.SuppressNamedInsureds = model.SuppressNamedInsureds;
                policy.SuppressEntities = model.SuppressEntities;
                policy.SuppressLocations = model.SuppressLocations;
                policy.SuppressLocationsNotScheduled = model.SuppressLocations;
                policy.AnualPremium = model.AnualPremium;
                policy.MinimumDeposit = model.MinimumDeposit;
                policy.MinimumEarned = model.MinimumEarned;

                _context.Policies.Update(policy);
                await _context.SaveChangesAsync();

                // Remove existing related entities in bulk
                await _context.PolicyEntities.Where(pe => pe.PolicyId == policyId).ExecuteDeleteAsync();
                await _context.PolicyLocations.Where(pl => pl.PolicyId == policyId).ExecuteDeleteAsync();

                // Insert new related entities
                if (model.SelectedEntityId?.Any() == true)
                {
                    var newPolicyEntities = model.SelectedEntityId.Select(entityId => new PolicyEntity
                    {
                        PolicyId = policyId,
                        ClientId = model.ClientId,
                        EntityId = entityId,
                        AddDate = DateTime.UtcNow,
                        ChangeDate = DateTime.UtcNow,
                        ChangeUid = "sa",
                        AddUid = "sa"
                    });

                    await _context.PolicyEntities.AddRangeAsync(newPolicyEntities);
                }

                if (model.SelectedLocationId?.Any() == true)
                {
                    var newPolicyLocations = model.SelectedLocationId.Select(locationId => new PolicyLocation
                    {
                        PolicyId = policyId,
                        ClientId = model.ClientId,
                        LocationId = locationId,
                        AddDate = DateTime.UtcNow,
                        ChangeDate = DateTime.UtcNow,
                        ChangeUid = "sa",
                        AddUid = "sa"
                    });

                    await _context.PolicyLocations.AddRangeAsync(newPolicyLocations);
                }

                var existingMajors = await _context.PolicyMajors.Where(m => m.PolicyId == policyId).ToListAsync();

                // Handle PolicyMajors
                if (model.PolicyMajors?.Any() == true)
                {
                    var modelMajorIds = model.PolicyMajors.Select(m => m.MajorId).ToHashSet();

                    foreach (var major in model.PolicyMajors)
                    {
                        var existingMajor = existingMajors.FirstOrDefault(m => m.MajorId == major.MajorId);
                        if (existingMajor == null)
                        {
                            var newMajor = new PolicyMajor
                            {
                                PolicyId = policyId,
                                Name = major?.Name ?? "",
                                Comments = major.Comments,
                                Sequence = major.Sequence
                            };
                            await _context.PolicyMajors.AddAsync(newMajor);
                            await _context.SaveChangesAsync();

                            major.MajorId = newMajor.MajorId;
                        }
                        else
                        {
                            existingMajor.Name = major.Name;
                            existingMajor.Sequence = major.Sequence;
                            existingMajor.Comments = major.Comments;
                        }

                        // Handle PolicyMajorColDefs
                        if (major.PolicyMajorColDefs?.Any() == true)
                        {
                            var existingColDefs = await _context.PolicyMajorColDefs
                                .Where(x => x.MajorId == major.MajorId)
                                .ToListAsync();

                            var modelColDefIds = major.PolicyMajorColDefs.Select(m => m.ColumnDefId).ToHashSet();

                            foreach (var policyMajor in major.PolicyMajorColDefs)
                            {
                                var existingColDef = existingColDefs.FirstOrDefault(cd => cd.ColumnDefId == policyMajor.ColumnDefId);
                                if (existingColDef == null)
                                {
                                    var newColDef = new PolicyMajorColDef
                                    {
                                        MajorId = major.MajorId ?? 0,
                                        ColumnName = policyMajor.ColumnName,
                                        Sequence = policyMajor.Sequence ?? 0,
                                        ColumnType = policyMajor?.ColumnType ?? "",
                                        Width = 0,
                                    };
                                    await _context.PolicyMajorColDefs.AddAsync(newColDef);
                                    await _context.SaveChangesAsync();

                                    policyMajor.ColumnDefId = newColDef.ColumnDefId;
                                }
                                else
                                {
                                    existingColDef.ColumnName = policyMajor.ColumnName;
                                    existingColDef.Sequence = policyMajor.Sequence ?? 0;
                                }

                                // Handle PolicyMinorDefs
                                if (policyMajor.PolicyMinorDefs?.Any() == true)
                                {
                                    var existingMinorDefs = await _context.PolicyMinorDefs
                                        .Where(x => x.ColumnDefId == policyMajor.ColumnDefId)
                                        .ToListAsync();

                                    var modelMinorIds = policyMajor.PolicyMinorDefs.Select(m => m.MinorId).ToHashSet();

                                    foreach (var policyMinor in policyMajor.PolicyMinorDefs)
                                    {
                                        var existingMinor = existingMinorDefs.FirstOrDefault(md => md.MinorId == policyMinor.MinorId);
                                        if (existingMinor == null)
                                        {
                                            var newMinorDef = new PolicyMinorDef
                                            {
                                                ColumnDefId = policyMajor.ColumnDefId ?? 0,
                                                RowSequence = policyMinor.RowSequence ?? 0,
                                                ColumnValue = policyMinor?.ColumnValue ?? "",
                                            };
                                            await _context.PolicyMinorDefs.AddAsync(newMinorDef);
                                            await _context.SaveChangesAsync();

                                            policyMinor.MinorId = newMinorDef.MinorId;
                                        }
                                        else
                                        {
                                            existingMinor.RowSequence = policyMinor.RowSequence ?? 0;
                                            existingMinor.ColumnValue = policyMinor.ColumnValue;
                                        }
                                    }

                                    // Delete missing PolicyMinorDefs
                                    var minorDefsToDelete = existingMinorDefs.Where(md => !modelMinorIds.Contains(md.MinorId)).ToList();
                                    if (minorDefsToDelete.Any())
                                    {
                                        _context.PolicyMinorDefs.RemoveRange(minorDefsToDelete);
                                    }
                                }
                            }

                            // Delete missing PolicyMajorColDefs and related PolicyMinorDefs
                            var colDefsToDelete = existingColDefs.Where(cd => !modelColDefIds.Contains(cd.ColumnDefId)).ToList();
                            if (colDefsToDelete.Any())
                            {
                                var minorDefsToDelete = await _context.PolicyMinorDefs
                                    .Where(md => colDefsToDelete.Select(cd => cd.ColumnDefId).Contains(md.ColumnDefId))
                                    .ToListAsync();

                                _context.PolicyMinorDefs.RemoveRange(minorDefsToDelete);
                                _context.PolicyMajorColDefs.RemoveRange(colDefsToDelete);
                            }
                        }
                    }

                    // Delete missing PolicyMajors and related data
                    var majorsToDelete = existingMajors.Where(m => !modelMajorIds.Contains(m.MajorId)).ToList();
                    if (majorsToDelete != null && majorsToDelete.Any())
                    {
                        foreach (var item in majorsToDelete)
                        {
                            var removeMinor = await _context.PolicyMajorColDefs.Where(x => x.MajorId == item.MajorId).ToListAsync();
                            foreach (var column in removeMinor)
                            {
                                var removeColumn = await _context.PolicyMinorDefs.Where(x => x.ColumnDefId == column.ColumnDefId).ToListAsync();

                                _context.PolicyMinorDefs.RemoveRange(removeColumn);
                            }
                            _context.PolicyMajorColDefs.RemoveRange(removeMinor);
                        }
                        _context.PolicyMajors.RemoveRange(majorsToDelete);
                    }
                }
                else
                {
                    //delete all major minor
                    if (existingMajors != null && existingMajors.Any())
                    {
                        foreach (var item in existingMajors)
                        {
                            var removeMinor = await _context.PolicyMajorColDefs.Where(x => x.MajorId == item.MajorId).ToListAsync();
                            foreach (var column in removeMinor)
                            {
                                var removeColumn = await _context.PolicyMinorDefs.Where(x => x.ColumnDefId == column.ColumnDefId).ToListAsync();

                                _context.PolicyMinorDefs.RemoveRange(removeColumn);
                            }
                            _context.PolicyMajorColDefs.RemoveRange(removeMinor);
                        }
                        _context.PolicyMajors.RemoveRange(existingMajors);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return policy;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TemplatePrincipal> UpdateTemplatePrincipal(TemplateRequest model)
        {
            if (model.TemplatePrincipal == null)
            {
                throw new ArgumentException("Invalid Policy ID.");
            }

            int principalId = model.TemplatePrincipal.PrincipalId??0;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dbTemplatePrincipal = await _context.TemplatePrincipals.FindAsync(principalId);
                if (dbTemplatePrincipal == null)
                {
                    throw new KeyNotFoundException("Policy not found.");
                }


                dbTemplatePrincipal.Name = model.TemplatePrincipal.Name??"";
                dbTemplatePrincipal.Description = model.TemplatePrincipal.Description ?? "";

                _context.TemplatePrincipals.Update(dbTemplatePrincipal);
                await _context.SaveChangesAsync();

                var existingMajors = await _context.TemplateMajors.Where(m => m.PrincipalId == principalId).ToListAsync();

                // Handle PolicyMajors
                if (model.TemplateMajor?.Any() == true)
                {
                    var modelMajorIds = model.TemplateMajor.Select(m => m.MajorId).ToHashSet();

                    foreach (var major in model.TemplateMajor)
                    {
                        var existingMajor = existingMajors.FirstOrDefault(m => m.MajorId == major.MajorId);
                        if (existingMajor == null)
                        {
                            var newMajor = new TemplateMajor
                            {
                                PrincipalId = principalId,
                                Name = major?.Name ?? "",
                                Comments = major?.Comments,
                                Sequence = major?.Sequence
                            };  
                            await _context.TemplateMajors.AddAsync(newMajor);
                            await _context.SaveChangesAsync();

                            major.MajorId = newMajor.MajorId;
                        }
                        else
                        {
                            existingMajor.Name = major?.Name??"";
                            existingMajor.Sequence = major?.Sequence;
                            existingMajor.Comments = major?.Comments;
                        }

                        if (major?.TemplateMajorColDef?.Any() == true)
                        {
                            var existingColDefs = await _context.TemplateMajorColDefs
                                .Where(x => x.MajorId == major.MajorId)
                                .ToListAsync();

                            var modelColDefIds = major.TemplateMajorColDef.Select(m => m.ColumnDefId).ToHashSet();

                            foreach (var templateMajor in major.TemplateMajorColDef)
                            {
                                var existingColDef = existingColDefs.FirstOrDefault(cd => cd.ColumnDefId == templateMajor.ColumnDefId);
                                if (existingColDef == null)
                                {
                                    var newColDef = new TemplateMajorColDef
                                    {
                                        MajorId = major.MajorId ?? 0,
                                        ColumnName = templateMajor.ColumnName,
                                        Sequence = templateMajor.Sequence ?? 0,
                                        ColumnType = templateMajor?.ColumnType ?? "",
                                        Width = 0,
                                    };
                                    await _context.TemplateMajorColDefs.AddAsync(newColDef);
                                    await _context.SaveChangesAsync();

                                    templateMajor.ColumnDefId = newColDef.ColumnDefId;
                                }
                                else
                                {
                                    existingColDef.ColumnName = templateMajor.ColumnName;
                                    existingColDef.Sequence = templateMajor.Sequence ?? 0;
                                }

                                if (templateMajor?.TemplateMinorDefs?.Any() == true)
                                {
                                    var existingMinorDefs = await _context.TemplateMinorDefs
                                        .Where(x => x.ColumnDefId == templateMajor.ColumnDefId)
                                        .ToListAsync();

                                    var modelMinorIds = templateMajor.TemplateMinorDefs.Select(m => m.MinorId).ToHashSet();

                                    foreach (var templateMinor in templateMajor.TemplateMinorDefs)
                                    {
                                        var existingMinor = existingMinorDefs.FirstOrDefault(md => md.MinorId == templateMinor.MinorId);
                                        if (existingMinor == null)
                                        {
                                            var newMinorDef = new TemplateMinorDef
                                            {
                                                ColumnDefId = templateMajor.ColumnDefId ?? 0,
                                                RowSequence = templateMinor.RowSequence ?? 0,
                                                ColumnValue = templateMinor?.ColumnValue ?? "",
                                            };
                                            await _context.TemplateMinorDefs.AddAsync(newMinorDef);
                                            await _context.SaveChangesAsync();
                                        }
                                        else
                                        {
                                            existingMinor.RowSequence = templateMinor.RowSequence ?? 0;
                                            existingMinor.ColumnValue = templateMinor.ColumnValue;
                                        }
                                    }

                                    var minorDefsToDelete = existingMinorDefs.Where(md => !modelMinorIds.Contains(md.MinorId)).ToList();
                                    if (minorDefsToDelete.Any())
                                    {
                                        _context.TemplateMinorDefs.RemoveRange(minorDefsToDelete);
                                    }
                                }
                            }

                            var colDefsToDelete = existingColDefs.Where(cd => !modelColDefIds.Contains(cd.ColumnDefId)).ToList();
                            if (colDefsToDelete.Any())
                            {
                                _context.TemplateMajorColDefs.RemoveRange(colDefsToDelete);
                            }
                        }
                    }

                    var majorsToDelete = existingMajors.Where(m => !modelMajorIds.Contains(m.MajorId)).ToList();
                    if (majorsToDelete != null && majorsToDelete.Any())
                    {
                        foreach (var item in majorsToDelete)
                        {
                            var removeMinor = await _context.TemplateMajorColDefs.Where(x => x.MajorId == item.MajorId).ToListAsync();
                            foreach (var column in removeMinor)
                            {
                                var removeColumn = await _context.TemplateMinorDefs.Where(x=>x.ColumnDefId  == column.ColumnDefId).ToListAsync();

                                _context.TemplateMinorDefs.RemoveRange(removeColumn);
                            }
                            _context.TemplateMajorColDefs.RemoveRange(removeMinor);
                        }
                        _context.TemplateMajors.RemoveRange(majorsToDelete);
                    }
                }
                else
                {
                    //delete all major minor
                    if (existingMajors != null && existingMajors.Any())
                    {
                        foreach (var item in existingMajors)
                        {
                            var removeMinor = await _context.TemplateMajorColDefs.Where(x => x.MajorId == item.MajorId).ToListAsync();
                            foreach (var column in removeMinor)
                            {
                                var removeColumn = await _context.TemplateMinorDefs.Where(x => x.ColumnDefId == column.ColumnDefId).ToListAsync();

                                _context.TemplateMinorDefs.RemoveRange(removeColumn);
                            }
                            _context.TemplateMajorColDefs.RemoveRange(removeMinor);
                        }
                        _context.TemplateMajors.RemoveRange(existingMajors);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return dbTemplatePrincipal;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<PolicyMajorColDef> UpdatePolicyMajorColDef(PolicyMajorModel model)
        {

            return null;
        }
        public async Task<PolicyModel> GetPolicyByIdAsync(int id)
        {
            try
            {
                var policy = await _context.Policies
                    .AsSplitQuery()
                    .Include(x => x.Broker)
                    .Include(x => x.Carrier)
                    .Include(x => x.PolicyEntities)
                    .Include(x => x.PolicyLocations)
                    .Include(x => x.Client)
                    .ThenInclude(x => x.ClientLocations)
                    .Include(x => x.Client)
                    .ThenInclude(x => x.ClientEntities)
                    .Include(x =>x.Principal)
                    .FirstOrDefaultAsync(x => x.PolicyId == id);

                if (policy == null)
                    return null;

                var policyData = _mapper.Map<PolicyModel>(policy);

                var policyType = await _context.PolicyTypes.ToListAsync();
                policyData.PolicyTypeName = policyType.Where(x => x.PolicyTypeId == policyData.PolicyType).FirstOrDefault()?.PolicyTypeName ?? "";

                //var majors = await _context.PolicyMajors.Where(x => x.PolicyId == policy.PolicyId).Include(y => y.PolicyMajorColDefs)
                //    .ThenInclude(colDef => colDef.PolicyMinorDefs).ToListAsync();

                var majors = await _context.PolicyMajors
                            .Where(x => x.PolicyId == policy.PolicyId)
                            .OrderBy(x => x.Sequence) // Order PolicyMajors by Sequence
                            .Include(y => y.PolicyMajorColDefs.OrderBy(colDef => colDef.Sequence)) // Order PolicyMajorColDefs by Sequence
                                .ThenInclude(colDef => colDef.PolicyMinorDefs
                                .OrderBy(minor => minor.RowSequence))
                            .ToListAsync();


                foreach (var major in majors)
                {
                    foreach (var colDef in major.PolicyMajorColDefs)
                    {
                        int[] seq = major.PolicyMajorColDefs.First().PolicyMinorDefs.Where(x=>!string.IsNullOrEmpty(x.ColumnValue)).Select(x => x.RowSequence).ToArray();
                        colDef.PolicyMinorDefs = colDef.PolicyMinorDefs.Where(x => seq.Contains(x.RowSequence)).ToList();
                    }
                }

                policyData.PolicyMajors = majors;
                policyData.PolicyMajors.FirstOrDefault();

                // Fetch Client Locations for PolicyLocations in a single query
                var locationIds = policyData.PolicyLocations.Select(pl => pl.LocationId).ToList();
                var clientLocations = await _context.ClientLocations
                    .Where(cl => locationIds.Contains(cl.LocationId))
                    .ToListAsync();
                policyData.ClientLocations = clientLocations;

                // Fetch Client Entities for PolicyEntities in a single query
                var entityIds = policyData.PolicyEntities.Select(pe => pe.EntityId).ToList();
                var clientEntities = await _context.ClientEntities
                    .Where(ce => entityIds.Contains(ce.EntityId))
                    .ToListAsync();
                policyData.ClientEntities = clientEntities;

                return policyData;
            }
            catch (Exception ex)
            {
                // Log the exception (assuming you have a logging mechanism)
                Console.WriteLine($"Error fetching policy: {ex.Message}");
                throw; // Rethrow the exception for further handling
            }
        }

        public async Task<TemplatePrincipalModel> GetTemplatePrincipalByIdAsync(int id)
        {
            try
            {
                //var templatePrincipal = await _context.TemplatePrincipals
                //    .AsSplitQuery()
                //    .Include(x=>x.Policies)
                //    //.IgnoreAutoIncludes()
                //    .Include(x => x.TemplateMajors)
                //    .ThenInclude(x => x.TemplateMajorColDefs)
                //    .ThenInclude(x => x.TemplateMinorDefs)
                //    .FirstOrDefaultAsync(x => x.PrincipalId == id);

                var templatePrincipal = await _context.TemplatePrincipals
                                        .AsSplitQuery()
                                        .Where(x => x.PrincipalId == id)
                                        .Select(x => new TemplatePrincipalModel
                                        {
                                            PrincipalId  = x.PrincipalId,
                                            Name = x.Name,
                                            Description = x.Description,
                                            Policies = x.Policies.Select(p => new Policy
                                            {
                                               PolicyId =  p.PolicyId,
                                               PolicyNo = p.PolicyNo,
                                               PolicyTitle = p.PolicyTitle,
                                               PolicyComment = p.PolicyComment
                                            }).ToList(),
                                            TemplateMajors = x.TemplateMajors.Select(tm => new TemplateMajor
                                            {
                                                MajorId =  tm.MajorId,
                                                Name = tm.Name,
                                                Sequence = tm.Sequence,
                                                Comments = tm.Comments,
                                                TemplateMajorColDefs = tm.TemplateMajorColDefs.Select(colDef => new TemplateMajorColDef
                                                {
                                                    ColumnDefId =  colDef.ColumnDefId,
                                                    ColumnName = colDef.ColumnName,
                                                    Sequence = colDef.Sequence,
                                                    TemplateMinorDefs = colDef.TemplateMinorDefs.Select(minor => new TemplateMinorDef
                                                    {
                                                        MinorId =  minor.MinorId,
                                                        RowSequence = minor.RowSequence,
                                                        ColumnValue = minor.ColumnValue
                                                    }).ToList()
                                                }).ToList()
                                            }).ToList()
                                        })
                                        .FirstOrDefaultAsync();


                if (templatePrincipal == null)
                    return null;

                var templatePrincipalData = _mapper.Map<TemplatePrincipalModel>(templatePrincipal);
                return templatePrincipalData;
            }
            catch (Exception ex)
            {
                // Log the exception (assuming you have a logging mechanism)
                Console.WriteLine($"Error fetching template Principal Data: {ex.Message}");
                throw; // Rethrow the exception for further handling
            }
        }


        /// <summary>
        /// Delete Policy
        /// </summary>
        /// <param name="policyId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePolicy(int policyId)
        {
            try
            {
                var policy = await _context.Policies.FindAsync(policyId);
                if (policy == null)
                {
                    return false; // policy does not exist
                }

                // Remove related entities using RemoveRange for better performance
                _context.PolicyEntities.RemoveRange(
                    await _context.PolicyEntities.Where(c => c.PolicyId == policyId).ToListAsync());

                _context.PolicyLocations.RemoveRange(
                    await _context.PolicyLocations.Where(c => c.PolicyId == policyId).ToListAsync());

                // Delete PolicyMinorDefs
                var minorDefs = await _context.PolicyMinorDefs
                    .Where(md => _context.PolicyMajorColDefs.Any(cd => cd.ColumnDefId == md.ColumnDefId
                        && _context.PolicyMajors.Any(m => m.MajorId == cd.MajorId && m.PolicyId == policyId)))
                    .ToListAsync();
                _context.PolicyMinorDefs.RemoveRange(minorDefs);

                // Delete TemplateMajorColDefs
                var colDefs = await _context.PolicyMajorColDefs
                    .Where(cd => _context.PolicyMajors.Any(m => m.MajorId == cd.MajorId && m.PolicyId == policyId))
                    .ToListAsync();
                _context.PolicyMajorColDefs.RemoveRange(colDefs);

                // Delete TemplateMajors
                var majors = await _context.PolicyMajors.Where(m => m.PolicyId == policyId).ToListAsync();
                _context.PolicyMajors.RemoveRange(majors);

                // Remove the policy itself
                _context.Policies.Remove(policy);

                // Save changes
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (Replace with actual logging)
                Console.WriteLine($"Error deleting policy: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templatePrincipalId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTemplatePrincipal(int principalId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dbTemplatePrincipal = await _context.TemplatePrincipals.FindAsync(principalId);
                if (dbTemplatePrincipal == null)
                {
                    throw new KeyNotFoundException("Policy not found.");
                }

                // Delete related records in Policy table first
                //var policies = await _context.Policies.Where(p => p.PrincipalId == principalId).ToListAsync();
                //_context.Policies.RemoveRange(policies);

                // Delete TemplateMinorDefs
                var minorDefs = await _context.TemplateMinorDefs
                    .Where(md => _context.TemplateMajorColDefs.Any(cd => cd.ColumnDefId == md.ColumnDefId
                        && _context.TemplateMajors.Any(m => m.MajorId == cd.MajorId && m.PrincipalId == principalId)))
                    .ToListAsync();
                _context.TemplateMinorDefs.RemoveRange(minorDefs);

                // Delete TemplateMajorColDefs
                var colDefs = await _context.TemplateMajorColDefs
                    .Where(cd => _context.TemplateMajors.Any(m => m.MajorId == cd.MajorId && m.PrincipalId == principalId))
                    .ToListAsync();
                _context.TemplateMajorColDefs.RemoveRange(colDefs);

                // Delete TemplateMajors
                var majors = await _context.TemplateMajors.Where(m => m.PrincipalId == principalId).ToListAsync();
                _context.TemplateMajors.RemoveRange(majors);

                // Delete TemplatePrincipal
                _context.TemplatePrincipals.Remove(dbTemplatePrincipal);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }



        /// <summary>
        /// Get policy by clientId
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<PolicyMasterListModel>> GetPolicyByClientId(int clientId, int pageNumber = 1, int pageSize = 100)
        {
            //var query = _context.Policies.Where(x => x.ClientId == clientId).AsQueryable();
            var query = _context.Policies
            .Where(x=>x.ClientId == clientId)
            .Select(x => new PolicyMasterListModel
            {
                PolicyId = x.PolicyId,
                PolicyNo = x.PolicyNo,
                PolicyTitle = x.PolicyTitle,
                PolicyType = x.PolicyType,
                Client = x.Client,
                Carrier = x.Carrier,
                PrintSequence = x.PrintSequence
            }).AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var listModel = _mapper.Map<List<PolicyMasterListModel>>(items);

            return new PaginatedResult<PolicyMasterListModel>
            {
                Items = listModel,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

        }

        /// <summary>
        /// Update policy expiration date
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Policy</returns>
        public async Task<Policy> UpdatePolicyExpiration(PolicyExpirationRequest model)
        {
            var policy = await _context.Policies.FirstOrDefaultAsync(x => x.PolicyId == model.PolicyId);

            if (policy == null)
            {
                throw new ArgumentException("Invalid Policy ID.");
            }

            if (model.RenewPolicy == true)
            {
                policy.Expired = false;
                policy.ExpirationDate = policy.ExpirationDate?.AddYears(1);
                policy.InceptionDate = policy.InceptionDate?.AddYears(1);
            }
            else
            {
                policy.Expired = true;
                //policy.ExpirationDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return policy;
        }

        /// <summary>
        /// Delete Minor 
        /// </summary>
        /// <param name="minorId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteMinor(List<int> minorIds)
        {
            try
            {
                if (minorIds == null || !minorIds.Any())
                    return false;

                // Get the matching major column definitions
                var majorColDefs = await _context.PolicyMajorColDefs
                    .Where(m => minorIds.Contains(m.ColumnDefId))
                    .ToListAsync();

                if (!majorColDefs.Any())
                {
                    return false; // No matching records found
                }

                // Get and remove all related minor definitions
                var relatedMinorDefs = await _context.PolicyMinorDefs
                    .Where(c => minorIds.Contains(c.ColumnDefId))
                    .ToListAsync();

                _context.PolicyMinorDefs.RemoveRange(relatedMinorDefs);

                // Remove the major column definitions
                _context.PolicyMajorColDefs.RemoveRange(majorColDefs);

                // Save changes to database
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting minor(s): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete Minor 
        /// </summary>
        /// <param name="minorId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteMinorRowByRowSequence(DeleteMinorRequest request)
        {
            try
            {
                if (request == null)
                    return false;

                // Get the matching major column definitions
                var majorColDefs = await _context.PolicyMajorColDefs
                    .Where(m => m.MajorId == request.MajorId)
                    .Select(m => m.ColumnDefId)
                    .ToListAsync();


                //get to delete PolicyMinorDefs IDs

                var policyMinorDefs = await _context.PolicyMinorDefs.
                     Where(p => majorColDefs.Contains(p.ColumnDefId) && p.RowSequence == request.RowSequence)
                     .ToListAsync();

                if (policyMinorDefs.Any())
                {
                    _context.PolicyMinorDefs.RemoveRange(policyMinorDefs);
                    _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting minor(s): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update policy Print sequence 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePolicyPrintSequence(UpdatePolicyPrientSequenceRequest model)
        {
            int sequenceNo = 1;

            foreach (var policyId in model.PolicyIdsList)
            {
                var policy = await _context.Policies.FirstOrDefaultAsync(x => x.PolicyId == policyId);
                if (policy != null)
                {
                    policy.PrintSequence = sequenceNo;
                    sequenceNo++;
                }

            }

            await _context.SaveChangesAsync();
            return true;
        }

    }

}
