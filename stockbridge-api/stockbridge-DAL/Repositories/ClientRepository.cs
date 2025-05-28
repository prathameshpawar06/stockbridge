using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using stockbridge_DAL.domainModels;
using stockbridge_DAL.DTOs;

namespace stockbridge_DAL.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly StockbridgeContext _context;
        private readonly IMapper _mapper;

        public ClientRepository(StockbridgeContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<ClientDTO>> GetClientsPaginatedAsync(ClientFilterRequest request)
        {
            var query = _context.Clients
                    .Include(c => c.ClientLocations)
                    .Include(c => c.ClientContacts)
                    .AsQueryable();

            query = query.Where(c => !string.IsNullOrEmpty(c.CompanyName));

            // Apply Filters
            if (!string.IsNullOrEmpty(request.SearchQuery))
                query = query.Where(c => c.CompanyName.Contains(request.SearchQuery));

            if (request.IsActive == true)
                query = query.Where(c => c.Active == request.IsActive.Value);

            if (request.IsNonActive == true)
                query = query.Where(c => c.Active != request.IsNonActive.Value);

            if (request.IsRetainerAccount == true)
                query = query.Where(c => c.RetainerAccount == request.IsRetainerAccount.Value);

            if (request.IsNonRetainer == true)
                query = query.Where(c => c.RetainerAccount != request.IsNonRetainer.Value);

            // Apply Sorting
            query = request.SortBy switch
            {
                "Name" => request.IsAscending ? query.OrderBy(c => c.CompanyName) : query.OrderByDescending(c => c.CompanyName),
                "Date created" => request.IsAscending ?
                               query.OrderBy(c => c.CreatedDate)
                               : query.OrderByDescending(c => c.CreatedDate),
                "Date modified" => request.IsAscending ?
                                query.OrderBy(c => c.ModifiedDate)
                                : query.OrderByDescending(c => c.ModifiedDate),
                "ClientAcctId" => request.IsAscending ? query.OrderBy(c => c.ClientAcctId) : query.OrderByDescending(c => c.ClientAcctId),
                "Status" => request.IsAscending ? query.OrderBy(c => c.Active) : query.OrderByDescending(c => c.Active),
                "RetainerAccount" => request.IsAscending ? query.OrderBy(c => c.RetainerAccount) : query.OrderByDescending(c => c.RetainerAccount),
                "PrimaryLocation" => request.IsAscending
                    ? query.OrderBy(c => c.ClientLocations.FirstOrDefault().Address1)  // Adjust as per actual field
                    : query.OrderByDescending(c => c.ClientLocations.FirstOrDefault().Address1),
                _ => query.OrderBy(c => c.ClientId), // Default sort,

            };

            // Pagination
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var clientDTOs = _mapper.Map<List<ClientDTO>>(items);

            // Preload consultants to avoid multiple queries in the loop
            var consultantIds = clientDTOs.Select(c => c.PrimaryConsultant).Distinct().ToList();
            var consultants = await _context.Staff
                .Where(s => consultantIds.Contains(s.StaffId))
                .ToDictionaryAsync(s => s.StaffId, s => s.Name);

            foreach (var client in clientDTOs)
            {
                var contact = client.ClientContacts.FirstOrDefault(x => x.ContactId == client.BillToContact);

                client.BillToContactName = contact?.FirstName + contact?.LastName ?? string.Empty;
                client.Contacts = new List<Contact>();

                ////Consultants name 
                //var staff = await _context.Staff.FirstOrDefaultAsync(x => x.StaffId == client.PrimaryConsultant);
                //if(staff != null)
                //{
                //    client.ConsultantName = staff.Name;
                //}

                // Assign consultant name
                client.ConsultantName = consultants.TryGetValue(client.PrimaryConsultant, out var consultantName)
                    ? consultantName
                    : string.Empty;
            }

            return new PaginatedResult<ClientDTO>
            {
                Items = clientDTOs,
                TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
            };
        }

        /// <summary>
        /// To get all Staff
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<Staff>> GetStaffAsync(string name = null, int pageNumber = 1, int pageSize = 100, int? clientId = null, bool? isActive = false,bool? IsNonActive = false)
        {
            var query = _context.Staff
                        .AsQueryable();

            if(isActive == true)
            {
                query = query.Where(x => x.Status == "A");
            }

            if(IsNonActive == true)
            {
                query = query.Where(x => x.Status == "N");
            }

            if (clientId > 0)
            {
                var clientStaff = _context.ClientStaffs.Where(x => x.ClientId == clientId).Select(x => x.StaffId);
                query = query.Where(x => clientStaff.Contains(x.StaffId));
            }

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name.Contains(name));

            var totalItems = await query.CountAsync();

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PaginatedResult<Staff>
            {
                Items = items,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }

        /// <summary>
        /// Get the staff by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StaffViewModel> GetStaffByIdAsync(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .AsSplitQuery()
                    .Include(x => x.ClientStaffs)
                    .ThenInclude(x => x.Client)
                    .ThenInclude(x => x.Policies)
                    //.ThenInclude(x => x.Policies)
                    //.Include(x => x.TimeData)
                    //.ThenInclude(x => x.Client)
                    .FirstOrDefaultAsync(x => x.StaffId == id);

                if (staff == null)
                    return null;

                var staffData = _mapper.Map<StaffViewModel>(staff);

                foreach (var client in staff.ClientStaffs)
                {
                    staffData.ClientsList.Add(client.Client);
                };

                return staffData;
            }
            catch (Exception ex)
            {
                // Log the exception (assuming you have a logging mechanism)
                Console.WriteLine($"Error fetching staff: {ex.Message}");
                throw; // Rethrow the exception for further handling
            }
        }

        /// <summary>
        /// Add staff
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Staff> AddStaff(StaffRequest model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Corrected mapping
            var staff = _mapper.Map<Staff>(model);
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync(); // Ensure staff.StaffId is set

            // Ensure ClientIds is not null
            if (model.ClientIds != null && model.ClientIds.Any())
            {
                foreach (var clientId in model.ClientIds)
                {
                    var clientStaff = new ClientStaff
                    {
                        ClientId = clientId,
                        StaffId = staff.StaffId // Ensure StaffId is properly assigned
                    };

                    _context.ClientStaffs.Add(clientStaff);
                }

                await _context.SaveChangesAsync(); // Make this async
            }

            return staff;
        }


        /// <summary>
        /// Update staff to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Staff> UpdateStaff(StaffRequest model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var staff = await _context.Staff.FindAsync(model.StaffId);
                if (staff == null)
                    throw new Exception("Staff not found.");

                // Update staff details
                _mapper.Map(model, staff);
                _context.Staff.Update(staff);
                await _context.SaveChangesAsync();

                // Delete existing ClientStaff entries
                var deleteStaffClient = await _context.ClientStaffs
                    .Where(x => x.StaffId == model.StaffId)
                    .ToListAsync();
                _context.ClientStaffs.RemoveRange(deleteStaffClient);

                // Add new ClientStaff entries
                if (model.ClientIds != null && model.ClientIds.Any())
                {
                    var clientStaffs = model.ClientIds.Select(clientId => new ClientStaff
                    {
                        ClientId = clientId,
                        StaffId = staff.StaffId
                    }).ToList();

                    await _context.ClientStaffs.AddRangeAsync(clientStaffs);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Commit transaction

                return staff;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback if any error occurs
                throw new Exception("An error occurred while updating staff.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> UpdateTimeData(List<TimeDatumRequest> modelList)
        {
            if (modelList == null || !modelList.Any())
                throw new ArgumentException("Model list cannot be null or empty.", nameof(modelList));

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var email = modelList.FirstOrDefault()?.Email;
                if (string.IsNullOrEmpty(email))
                    throw new ArgumentException("Email cannot be null or empty.", nameof(email));

                var staff = await _context.Staff.FirstOrDefaultAsync(x => x.Email == email);
                if (staff == null)
                    throw new Exception("Staff not found for the provided email.");

                var staffId = staff.StaffId;

                foreach (var model in modelList)
                {
                    var timedata = await _context.TimeSheet.FindAsync(model.TimeSheetId);

                    if (timedata == null)
                    {
                        var timeData = _mapper.Map<TimeSheet>(model);
                        timeData.StaffId = staffId;
                        timeData.CreatedDate = DateTime.Now;
                        _context.TimeSheet.Add(timeData);
                    }
                    else
                    {
                        _mapper.Map(model, timedata);
                        timedata.CreatedDate = DateTime.Now;
                        timedata.StaffId = staffId;
                        _context.TimeSheet.Update(timedata);
                    }

                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Commit transaction
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback if any error occurs
                throw new Exception("An error occurred while updating TimeData.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>

        public async Task<List<TimeSheet>> GetTimeListData(int staffId)
        {
            try
            {
                var timedata = await _context.TimeSheet.Where(x => x.StaffId == staffId).ToListAsync();
                return timedata;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while getting TimeData.", ex);
            }
        }

        public async Task<int> GetTotalClientsCountAsync()
        {
            return await _context.Clients.CountAsync();
        }

        public async Task<ClientDTO> GetClientByIdAsync(int clientId)
        {
            var client = await _context.Clients
                .Include(c => c.ClientLocations)
                .Where(c => c.ClientId == clientId)
                .FirstOrDefaultAsync();

            var clientData = _mapper.Map<ClientDTO>(client);

            try
            {
                if (client != null)
                {
                    var clientContact = await _context.ClientContacts
                                        .Where(c => c.ClientId == clientId)
                                        .ToListAsync();
                    clientData.Contacts = _mapper.Map<List<Contact>>(clientContact);

                    var clientLocations = await _context.ClientLocations
                                        .Where(c => c.ClientId == clientId)
                                        .ToListAsync();
                    clientData.Locations = _mapper.Map<List<Location>>(clientLocations);

                    var clientEntities = await _context.ClientEntities
                                        .Where(c => c.ClientId == clientId)
                                        .ToListAsync();
                    clientData.Entities = _mapper.Map<List<Entities>>(clientEntities);

                    var clientStaff = await _context.ClientStaffs
                                        .Where(c => c.ClientId == clientId)
                                        .ToListAsync();

                    clientData.Staff = _mapper.Map<List<StaffModel>>(clientStaff);

                    var policyTypeLists = await _context.PolicyTypes.ToListAsync();
                    clientData.Policies = await _context.Policies
                                       .Where(c => c.ClientId == clientId).Select(c => new PolicyListModel
                                       {
                                           PolicyId = c.PolicyId,
                                           PolicyNo = c.PolicyNo,
                                           PolicyTitle = c.PolicyTitle,
                                           ExpirationDate = c.ExpirationDate,
                                           AnualPremium = c.AnualPremium,
                                           PolicyComment = c.PolicyComment,
                                           Description = c.Principal.Description ?? "",
                                           PolicyType = c.PolicyType,
                                           Expired = c.Expired,
                                       })
                                       .ToListAsync();

                    foreach (var item in clientData.Policies)
                    {
                        item.PolicyTypeName = policyTypeLists.Where(y => y.PolicyTypeId == item.PolicyType).Select(x => x.PolicyTypeName).FirstOrDefault();
                    }

                    foreach (var item in clientData.Staff)
                    {
                        var staff = await _context.Staff.FindAsync(item.StaffId);
                        if (staff != null)
                        {
                            item.Status = staff.Status;
                            item.Rate = staff.Rate;
                            item.Name = staff.Name;
                        }
                    }
                }

            }
            catch (Exception)
            {

            }

            return clientData;
        }

        /// <summary>
        /// Add client to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Client> AddClientAsync(ClientRequest model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(); // Begin the transaction

            try
            {
                var client = _mapper.Map<Client>(model.BasicTab);
                await _context.Clients.AddAsync(client);
                client.CreatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                foreach (var contacts in model.Contacts)
                {
                    var contact = _mapper.Map<ClientContact>(contacts);
                    contact.ClientId = client.ClientId;
                    _context.ClientContacts.Add(contact);
                }

                foreach (var location in model.Locations)
                {
                    var locations = _mapper.Map<ClientLocation>(location);
                    locations.ClientId = client.ClientId;
                    locations.Address1 = locations.Address1 ?? "";
                    locations.Address2 = locations.Address2 ?? "";
                    locations.LocationId = 0;
                    _context.ClientLocations.Add(locations);
                }

                foreach (var entities in model.Entities)
                {
                    var record = _mapper.Map<ClientEntity>(entities);
                    record.EntityId = 0;
                    record.ClientId = client.ClientId;
                    _context.ClientEntities.Add(record);
                }

                foreach (var staff in model.Staff)
                {
                    var clientStaff = new ClientStaff
                    {
                        ClientId = client.ClientId,
                        StaffId = staff.StaffId ?? 0
                    };
                    _context.ClientStaffs.Add(clientStaff);
                }

                // Save changes and commit the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();  // Commit transaction

                return client;
            }
            catch (Exception ex)
            {
                // Rollback in case of an error
                await transaction.RollbackAsync();
                throw ex;  // Rethrow the exception to be handled higher up
            }
        }

        /// <summary>
        /// Update an existing client in the database
        /// </summary>
        /// <param name="clientId">The ID of the client to update</param>
        /// <param name="model">The updated client data</param>
        /// <returns></returns>
        public async Task<Client> UpdateClientAsync(ClientRequest model)
        {
            var clientId = model?.BasicTab?.ClientId ?? 0;

            using var transaction = await _context.Database.BeginTransactionAsync(); // Begin transaction

            try
            {
                var client = await _context.Clients
                    .Include(c => c.ClientContacts)
                    .Include(c => c.ClientLocations)
                    .Include(c => c.ClientEntities)
                    .Include(c => c.ClientStaffs)
                    .FirstOrDefaultAsync(c => c.ClientId == clientId);

                if (client == null)
                {
                    throw new Exception("Client not found.");
                }

                // Update main client properties
                _mapper.Map(model.BasicTab, client);
                client.ModifiedDate = DateTime.Now;
                client.Fax = model?.BasicTab?.Email;
                _context.Clients.Update(client);

                // Update Contacts
                var existingContacts = client.ClientContacts.ToList();
                _context.ClientContacts.RemoveRange(existingContacts.ExceptBy(model.Contacts.Select(c => c.ContactId), c => c.ContactId));
                foreach (var contactModel in model.Contacts)
                {
                    var contact = existingContacts.FirstOrDefault(c => c.ContactId == contactModel.ContactId);
                    if (contact == null)
                    {
                        contact = _mapper.Map<ClientContact>(contactModel);
                        contact.ClientId = clientId;
                        _context.ClientContacts.Add(contact);
                    }
                    else
                    {
                        _mapper.Map(contactModel, contact);
                    }
                }

                // Update Locations
                var existingLocations = client.ClientLocations.ToList();
                _context.ClientLocations.RemoveRange(existingLocations.ExceptBy(model.Locations.Select(l => l.LocationId), l => l.LocationId));
                foreach (var locationModel in model.Locations)
                {
                    var location = existingLocations.FirstOrDefault(l => l.LocationId == locationModel.LocationId);
                    if (location == null)
                    {
                        location = _mapper.Map<ClientLocation>(locationModel);
                        location.ClientId = clientId;
                        location.Address1 = location.Address1 ?? "";
                        location.Address2 = location.Address2 ?? "";
                        location.LocationId = 0;
                        _context.ClientLocations.Add(location);
                    }
                    else
                    {
                        _mapper.Map(locationModel, location);
                    }
                }

                // Update Entities
                var existingEntities = client.ClientEntities.ToList();
                _context.ClientEntities.RemoveRange(existingEntities.ExceptBy(model.Entities.Select(e => e.EntityId), e => e.EntityId));
                foreach (var entityModel in model.Entities)
                {
                    var entity = existingEntities.FirstOrDefault(e => e.EntityId == entityModel.EntityId);
                    if (entity == null)
                    {
                        entity = _mapper.Map<ClientEntity>(entityModel);
                        entity.ClientId = clientId;
                        entity.EntityId = 0;
                        _context.ClientEntities.Add(entity);
                    }
                    else
                    {
                        _mapper.Map(entityModel, entity);
                    }
                }

                // Update Staff
                var existingStaff = client.ClientStaffs.ToList();
                _context.ClientStaffs.RemoveRange(existingStaff.ExceptBy(model.Staff.Select(s => s.StaffId ?? 0), s => s.StaffId));
                foreach (var staffModel in model.Staff)
                {
                    var staff = existingStaff.FirstOrDefault(s => s.StaffId == staffModel.StaffId);
                    if (staff == null)
                    {
                        var clientStaff = new ClientStaff
                        {
                            ClientId = clientId,
                            StaffId = staffModel.StaffId ?? 0
                        };
                        _context.ClientStaffs.Add(clientStaff);
                    }
                }

                // Save changes and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return client;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;  // Rethrow exception for handling
            }
        }

        /// <summary>
        /// Delete the client
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteClient(int clientId)
        {
            try
            {
                var client = await _context.Clients.FindAsync(clientId);
                if (client == null)
                {
                    return false; // Client does not exist
                }

                // Remove related entities using RemoveRange for better performance
                _context.ClientContacts.RemoveRange(
                    await _context.ClientContacts.Where(c => c.ClientId == clientId).ToListAsync());

                _context.ClientLocations.RemoveRange(
                    await _context.ClientLocations.Where(c => c.ClientId == clientId).ToListAsync());

                _context.ClientEntities.RemoveRange(
                    await _context.ClientEntities.Where(c => c.ClientId == clientId).ToListAsync());

                _context.ClientStaffs.RemoveRange(
                    await _context.ClientStaffs.Where(c => c.ClientId == clientId).ToListAsync());

                // Remove the client itself
                _context.Clients.Remove(client);

                // Save changes
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (Replace with actual logging)
                Console.WriteLine($"Error deleting client: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedResult<ClientContact>> GetClientContacts(ClientContactFilterRequest clientContactFilterRequestObj)
        {
            var query = _context.ClientContacts.Include(x => x.Client)
            .AsQueryable();

            //Apply Filters 

            if (!string.IsNullOrEmpty(clientContactFilterRequestObj.SearchQuery))
            {
                query = query.Where(x => x.FirstName.Contains(clientContactFilterRequestObj.SearchQuery) || x.LastName.Contains(clientContactFilterRequestObj.SearchQuery) || x.Client.CompanyName.Contains(clientContactFilterRequestObj.SearchQuery));
            }

            var totalItems = await query.CountAsync();

            var items = await query.Skip((clientContactFilterRequestObj.PageNumber - 1) * clientContactFilterRequestObj.PageSize)
                                .Take(clientContactFilterRequestObj.PageSize)
                                .ToListAsync();

            return new PaginatedResult<ClientContact>
            {
                Items = items,
                TotalPages = (int)Math.Ceiling((double)totalItems / clientContactFilterRequestObj.PageSize)
            };

        }

        public async Task<int> GetStaffIdByEmail(string email)
        {
            try
            {
                var staff = await _context.Staff.FirstOrDefaultAsync(x => x.Email == email);
                return staff == null ? 0 : staff.StaffId;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while getting TimeData.", ex);
            }
        }

    }
}
