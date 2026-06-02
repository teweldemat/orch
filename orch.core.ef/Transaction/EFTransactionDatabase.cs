using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using orch.common;
using orch.core.ef.Transaction;
using orch.core.ef.Transaction.Entities;
using orch.core.errors;
using orch.core.model;
using orch.core.model.dto;
using orch.ef.Core;
using System.Data;
using System.Data.Common;
using FuncScript.Core;
using FuncScript.Model;


namespace orch.core.ef.System
{
    [OView(Name = "core")]
    public class EFTransactionDatabase : ITransactionDatabase
    {
        private readonly OTransactionDbContext _db;
        private readonly List<ODbContext> _contexts;

        private readonly ISystemDatabase _systemDatabase;

        private DbTransaction? _dbTransaction;

        public EFTransactionDatabase(OTransactionDbContext db, ISystemDatabase systemDatabase)
        {
            _db = db;
            _contexts = new List<ODbContext>();
            _systemDatabase = systemDatabase;
        }

        /// <summary>
        /// Adds the specified <paramref name="context"/> to the list of database contexts to be included in the transaction.
        /// </summary>
        /// <param name="context">The database context to be included in the transaction.</param>
        /// <remarks>
        /// If a database transaction has been initiated, the <paramref name="context"/> will be associated with the transaction.
        /// </remarks>
        public void AddTransactionDBContext(ODbContext context)
        {
            if (_dbTransaction != null)
            {
                context.Database.UseTransaction(_dbTransaction);
            }
            if (!_contexts.Contains(context))
                _contexts.Add(context);
        }
        public bool InTransaction => _dbTransaction != null;

        /// <summary>
        /// Begins a new transaction for the transaction context and all associated contexts.
        /// </summary>
        public void BeginTransaction()
        {
            if (_dbTransaction != null) //I hope am not calling for trouble
                return;
            var con = _db.Connection;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            _dbTransaction = con.BeginTransaction();
            _db.Database.UseTransaction(_dbTransaction);
            foreach (var db in _contexts)
            {
                db.Database.UseTransaction(_dbTransaction);
            }
        }

        /// <summary>
        /// Detaches all entities currently tracked by the change tracker of the specified <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The database context whose entities are to be detached.</param>
        /// <remarks>
        /// If an entity is not in a detached state, its state is set to detached.
        /// </remarks>
        private static void DetachEntities(DbContext context)
        {
            try
            {
                foreach (var entityEntry in context.ChangeTracker.Entries())
                {
                    if (entityEntry.State != EntityState.Detached)
                    {
                        entityEntry.State = EntityState.Detached;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Detaches all entities currently tracked by the change tracker of the database context associated with this instance
        /// as well as all other contexts added to this instance.
        /// </summary>
        private void DetachFromAllContexts()
        {
            DetachEntities(_db);
            foreach (var context in _contexts)
                DetachEntities(context);
        }

        /// <summary>
        /// Commits the transaction associated with this instance,
        /// if one exists, and detaches all entities tracked by the change tracker of this instance
        /// as well as all other contexts added to this instance.
        /// </summary>
        public void CommitTransaction()
        {
            _dbTransaction?.Commit();
            _dbTransaction = null;
            DetachFromAllContexts();
        }

        /// <summary>
        /// Rolls back the transaction associated with this instance,
        /// if one exists, and detaches all entities tracked by the change tracker of this instance
        /// as well as all other contexts added to this instance.
        /// </summary>
        public void RollbackTransaction()
        {
            _dbTransaction?.Rollback();
            _dbTransaction = null;
            DetachFromAllContexts();
        }

        public void EnableReadOnlyMode()
        {
            _db.Mode = ODbContext.DatabaseMode.ReadOnly;
            foreach (var c in _contexts)
            {
                c.Mode = ODbContext.DatabaseMode.ReadOnly;
            }
        }

        public void DisableReadOnlyMode()
        {
            _db.Mode = ODbContext.DatabaseMode.ReadWrite;
            foreach (var c in _contexts)
            {

                c.Mode = ODbContext.DatabaseMode.ReadWrite;

            }
        }

        /// <summary>
        /// Adds the specified <paramref name="command"/> to the transaction database by creating a new <see cref="DALCommand"/> object
        /// and adding it to the corresponding DbSet of the <see cref="TransactionDbContext"/> instance.
        /// </summary>
        /// <param name="command">The command to be added to the transaction database.</param>
        public void AddCommand(OCommand command)
        {
            _db.Commands.Add(new DALCommand(command));
            _db.SaveChanges();
        }

        /// <summary>
        /// Adds the specified <paramref name="tranSet"/> to the transaction database by creating a new <see cref="DALOTransaction"/> object and adding it to the corresponding DbSet of the <see cref="TransactionDbContext"/> instance.
        /// </summary>
        /// <param name="tranSet">The transaction to be added to the transaction database.</param>
        public void AddTransaction(OTransaction tranSet)
        {

            _db.Transactions.Add(new DALOTransaction(tranSet));
            _db.SaveChanges();
        }

        public void AddJob(OJob job)
        {

            _db.Jobs.Add(new DALOJob(job));
            _db.SaveChanges();
        }

        /// <summary>
        /// Retrieves the head transaction from the transaction database,
        /// which is the most recent transaction that was added to the database.
        /// </summary>
        [OViewFunction]
        public OCommand? GetHeadTransaction()
        {
            var sysInfo = _db.TransactionSystemInformation.AsNoTracking().FirstOrDefault();
            if (sysInfo == null)
                return null;
            return _db.Commands.Where(command => command.TranId == sysInfo.HeadTranId)
                .OrderByDescending(c => c.SeqNo)
                .Select(command => new OCommand(command))
                .FirstOrDefault();
        }
        [OViewFunction]
        public TransactionSystemInformation? GetCurrentSystemInformation()
        {
            var sysInfo = _db.TransactionSystemInformation.AsNoTracking().FirstOrDefault();
            return sysInfo == null ? null : new TransactionSystemInformation(sysInfo);
        }

        public long LastTranSeqNo
        {
            get
            {
                // Retrieve the highest SeqNo from the transactions in the context.
                // If there are no transactions yet, default to 0.
                return _db.Transactions.Max(t => (int?)t.SeqNo) ?? 0;
            }
        }

        public long Count
        {
            [OViewFunction("GetTransactionsCount")]
            get => _db.Transactions.Count();
        }

        [OViewFunction("GetTransactionsCountByDataTypeIds")]
        public long CountByDataTypeIds(List<Guid> dataTypeIds)
        {
            return _db.Transactions
                .AsNoTracking()
                .Where(t => t.Commands.Any(c => c.TranId == t.Id && dataTypeIds.Contains(c.DataTypeID)))
                .Count();
        }

        [OViewFunction("LastSeqNoByDataTypeIds")]
        public long LastSeqNoByDataTypeIds(List<Guid> dataTypeIds)
        {
            return _db.Transactions
                .AsNoTracking()
                .Where(t => t.Commands.Any(c => c.TranId == t.Id && dataTypeIds.Contains(c.DataTypeID)))
                .Max(t => t.SeqNo);
        }

        public T? Deserialize<T>(OCommand command)
        {
            if (string.IsNullOrEmpty(command.TextData))
                return default;
            return JsonConvert.DeserializeObject<T>(command.TextData);
        }

        public UserInfo? GetRootUser()
        {
            return _db.Users.Include(userInfo => userInfo.Roles)
                        .AsNoTracking()
                        .Where(userInfo => userInfo.UserName.Equals(UserInfo.USER_NAME_ROOT))
                        .Select(userInfo => new UserInfo(userInfo)
                        {
                            PasswordHash = null,
                            Roles = userInfo.Roles.OrderBy(userRole => userRole.Order)
                            .Select(userRole => userRole.RoleId).ToList()
                        })
                        .FirstOrDefault();
        }

        public UserInfo? GetSystemUser()
        {
            return GetUserInfo(UserInfo.USER_NAME_SYSTEM, false);
        }

        public UserInfo? GetUserInfo(string userName, bool includePassword = false)
        {

            return _db.Users.AsNoTracking().Include(userInfo => userInfo.Roles).Where(userInfo => userInfo.UserName.ToUpper() == userName.ToUpper())
            .AsEnumerable()
            .Select(userInfo => new UserInfo(userInfo)
            {
                PasswordHash = includePassword ? userInfo.PasswordHash : null,
                Roles = userInfo.Roles.OrderBy(userRole => userRole.Order).Select(userRole => userRole.RoleId).ToList(),
                EmployeeId = userInfo.EmployeeId.HasValue ? userInfo.EmployeeId.Value : null,
                ReaderId = userInfo.ReaderId.HasValue ? userInfo.ReaderId.Value : null
            })
            .FirstOrDefault();
        }

        public UserInfo? GetUserInfo(Guid agentId, bool includePassword = false)
        {
            return _db.Users.Include(x => x.Roles).AsNoTracking().Where(userInfo => userInfo.Id == agentId)
                            .Select(userInfo => new UserInfo(userInfo)
                            {
                                PasswordHash = includePassword ? userInfo.PasswordHash : null,
                                Roles = userInfo.Roles.OrderBy(userRole => userRole.Order).Select(userRole => userRole.RoleId).ToList(),
                                EmployeeId = userInfo.EmployeeId.HasValue ? userInfo.EmployeeId.Value : null,
                                ReaderId = userInfo.ReaderId.HasValue ? userInfo.ReaderId.Value : null
                            })
                            .FirstOrDefault();
        }

        [OViewFunction]
        public UserInfo? GetUserByCommandId(Guid id)
        {
            return _db.Users.Include(userInfo => userInfo.Roles).Where(userInfo => userInfo.UpdateCommandId == id)
                        .AsEnumerable()
                        .Select(userInfo => new UserInfo(userInfo)
                        {
                            PasswordHash = null,
                            Roles = userInfo.Roles.OrderBy(userRole => userRole.Order).Select(userRole => userRole.RoleId).ToList(),
                            EmployeeId = userInfo.EmployeeId.HasValue ? userInfo.EmployeeId.Value : null,
                            ReaderId = userInfo.ReaderId.HasValue ? userInfo.ReaderId.Value : null
                        })
                        .FirstOrDefault();
        }

        public UserInfo? GetUserInfoByEmplyeeId(Guid employeeId)
        {
            return _db.Users.Include(x => x.Roles).AsNoTracking().Where(userInfo => userInfo.EmployeeId == employeeId)
                            .Select(userInfo => new UserInfo(userInfo)
                            {
                                PasswordHash = userInfo.PasswordHash,
                                Roles = userInfo.Roles.OrderBy(userRole => userRole.Order).Select(userRole => userRole.RoleId).ToList(),
                                EmployeeId = userInfo.EmployeeId.HasValue ? userInfo.EmployeeId.Value : null,
                                ReaderId = userInfo.ReaderId.HasValue ? userInfo.ReaderId.Value : null
                            })
                            .FirstOrDefault();
        }
        public void UpdateUserInfo(UserInfo agent)
        {

            var existing = GetUserInfo(agent.Id, true);
            if (existing == null)
            {
                throw new InvalidOperationException($"User with ID:{agent.Id} doesn't exist");
            }
            existing.FullName = agent.FullName;
            existing.EmployeeId = agent.EmployeeId;
            existing.ReaderId = agent.ReaderId;
            existing.Email = agent.Email;
            existing.PhoneNo = agent.PhoneNo;

            _db.Users.Update(new DALUserInfo(existing));
            _db.SaveChanges();
        }

        public void CreateUser(OCommand command, UserInfo agentInfo)
        {
            if (_db.Users.AsNoTracking().Any(userInfo => userInfo.UserName.ToUpper() == agentInfo.UserName.ToUpper()))
            {
                throw new InvalidOperationException($"Username '{agentInfo.UserName}' is already in use. Please choose a different username.");
            }

            agentInfo.SetCreate<ChangeProps>(command);
            _db.Users.Add(new DALUserInfo(agentInfo));
            if (agentInfo.Roles != null)
            {
                int n = 1;
                foreach (var role in agentInfo.Roles)
                {
                    _db.UserRoles.Add(new DALUserRole { UserId = agentInfo.Id, RoleId = role, Order = n });
                    n++;
                }
            }
            _db.UserHistory.Add(new DALUserHistory
            { UserId = agentInfo.Id, CommandId = agentInfo.CreateCommandId, CommandTime = agentInfo.Time });
            _db.SaveChanges();
        }

        /// <summary>
        /// Creates a new role with the given command and role object.
        /// </summary>
        /// <param name="command">The command to use when creating the role.</param>
        /// <param name="role">The role object to create.</param>
        /// 
        public void CreateRole(OCommand command, Role role)
        {
            role.SetCreate<ChangeProps>(command);
            if (GetRole(role.Key) != null)
            {
                throw new InvalidOperationException($"Role key {role.Key} already used");
            }
            _db.Roles.Add(new DALRole(role));
            if (role.Permissions != null)
            {
                var order = 1;
                foreach (var permissionId in role.Permissions)
                    _db.PermissionRoles.Add(
                        new DALRolePermission
                        {
                            Order = order++,
                            PermissionId = permissionId,
                            RoleId = role.Id
                        });
            }
            _db.SaveChanges();
        }

        /// <summary>
        /// Creates a new permission with the given command and permission object.
        /// </summary>
        /// <param name="command">The command to use when creating the permission.</param>
        /// <param name="permission">The permission object to create.</param>
        public void CreatePermission(OCommand command, Permission permssion)
        {
            permssion.SetCreate<ChangeProps>(command);
            if (_db.Permissions.FirstOrDefault
                (permission => permission.PermissionKey == permssion.PermissionKey) != null)
            {
                throw new InvalidOperationException($"Permission key {permssion.PermissionKey} already used");
            }
            _db.Permissions.Add(new DALPermission(permssion));
            _db.SaveChanges();
        }

        public void SetUserRole(OCommand command, Guid userId, List<Guid> roles)
        {
            var user = _db.Users.Where(userInfo => userInfo.Id == userId).FirstOrDefault();

            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not defined");
            }

            user.SetUpdate<ChangeProps>(command);
            var order = 1;
            _db.UserRoles.RemoveRange(_db.UserRoles.Where(x => x.UserId == userId));
            foreach (var roleId in roles)
            {
                _db.UserRoles.Add(new DALUserRole()
                {
                    Order = order,
                    UserId = userId,
                    RoleId = roleId,
                });
                order++;
            }
            _db.UserHistory.Add(new DALUserHistory()
            {
                UserId = userId,
                CommandId = command.Id,
                CommandTime = command.Time
            });
            _db.SaveChanges();
        }

        public Guid GetSystemUserId()
        {
            var user = GetUserInfo(UserInfo.USER_NAME_SYSTEM);
            if (user == null)
            {
                throw new InvalidOperationException("System user not defined");
            }
            return user.Id;
        }

        public void SetRolePermssions(OCommand command, Guid roleId, List<Guid> permssions)
        {
            var role = _db.Roles.Where(role => role.Id == roleId).FirstOrDefault();
            if (role == null)
            {
                throw new InvalidOperationException($"Role {roleId} not defined");
            }
            role.SetUpdate<ChangeProps>(command);
            _db.PermissionRoles.RemoveRange(_db.PermissionRoles.Where(rolePermission => rolePermission.RoleId == roleId));

            var order = 1;
            foreach (var permissionId in permssions)
            {
                _db.PermissionRoles.Add(new DALRolePermission()
                {
                    Order = order,
                    PermissionId = permissionId,
                    RoleId = roleId,
                });
                order++;
            }
            _db.SaveChanges();
        }

        public void ExecuteDml(OCommand command, string dml)
        {
            // TODO Possible security vulnerability
            _db.Database.ExecuteSqlRaw(dml);
        }

        [OViewFunction]
        public Role GetRole(Guid role)
        {
            var dalRole = _db.Roles
                .Include(role => role.Permissions)
                .Where(dalRole => dalRole.Id == role).FirstOrDefault();

            if (dalRole == null)
            {
                throw new InvalidOperationException($"Role {role} not defined");
            }
            return new Role(dalRole)
            {
                Permissions = dalRole.Permissions
                .OrderBy(rolePermission => rolePermission.Order)
                .Select(rolePermission => rolePermission.PermissionId).ToList()
            };
        }
        [OViewFunction("GetRoleByKey")]
        public Role? GetRole(string key)
        {
            return _db.Roles
                .Include(role => role.Permissions)
                .Where(role => role.Key == key)
                .Select(role => new Role(role)
                {
                    Permissions = role.Permissions.OrderBy(rolePermission => rolePermission.Order)
                    .Select(rolePermission => rolePermission.PermissionId).ToList()
                })
                .FirstOrDefault();
        }

        public List<Guid> GetUsersWithRoles(List<Guid> roleIds)
        {
            var roles = roleIds.Select(id => GetRole(id));

            if (roles.Any(role => role == null))
            {
                throw new InvalidOperationException($"One or more roles not defined");
            }

            var users = _db.UserRoles.AsNoTracking().AsEnumerable().Where(ur => roleIds.Contains(ur.RoleId))
                                     .Select(ur => ur.UserId)
                                     .Distinct()
                                     .ToList();

            return users;
        }



        [OViewFunction]
        public List<Guid> GetUserPermissions(Guid agentId)
        {
            var roles = _db.UserRoles.Where(x => x.UserId == agentId).ToList();
            var roleIds = new List<Guid>();
            foreach (var role in roles)
                roleIds.AddRange(from p in _db.PermissionRoles
                                 .Where(rolePermission => rolePermission.RoleId == role.RoleId)
                                 where !roleIds.Contains(p.PermissionId)
                                 select p.PermissionId);
            return roleIds;
        }

        [OViewFunction]
        public Permission? GetPermission(Guid permssionId)
        {
            return _db.Permissions.Where(permission => permission.Id == permssionId)
                .Select(permission => new Permission(permission))
                .FirstOrDefault();
        }

        [OViewFunction("GetPermissionByKey")]
        public Permission? GetPermission(string permKey)
        {
            return _db.Permissions.Where(permission => permission.PermissionKey == permKey)
                        .Select(permission => new Permission(permission))
                        .FirstOrDefault();
        }

        public List<Permission> ExpandPermssions(string permKey)
        {
            // TODO Implement ExpandPermssions
            throw new NotImplementedException();
        }

        public List<Guid> GetUserWithPermissions(IList<Guid> userWithPermissions)
        {
            var roles = _db.PermissionRoles.Where(rolePermission
                => userWithPermissions.Contains(rolePermission.PermissionId))
                    .Select(rolePermission => rolePermission.RoleId).Distinct();
            return _db.UserRoles.Where(userRole => roles.Contains(userRole.RoleId))
                    .Select(userRole => userRole.UserId).Distinct().ToList();
        }

        public SerialNo UseNextSerialNo(OCommand command, Guid batchId, KeyValueCollection? provider = null)
        {

            var batch = _db.SerialBatches.FirstOrDefault(serialBatch
                => serialBatch.Id == batchId);

            if (batch == null)
            {
                throw new InvalidOperationException($"Invalid serial batch id {batchId}");
            }
            var type = GetSerialType(batch.SerialTypeId);
            if (batch.MaxUsed >= batch.ToSerialNo)
            {
                throw new InvalidOperationException("Serial batch used up");
            }
            batch.MaxUsed++;
            var serialNo
                = new DALSerialNo
                {
                    BatchId = batchId,
                    Sn = batch.MaxUsed,
                    Formatted = type?.FormatSerialNo(batch.MaxUsed, provider),
                    IsVoid = false,
                }.SetCreate<DALSerialNo>(command);

            _db.UsedSerials.Add(serialNo);
            _db.SaveChanges();
            return new SerialNo(serialNo);
        }

        public PermissionAdminInfo GetPermissionAdminInfo(string pk)
        {
            // TODO implement GetPermissionAdminInfo
            throw new NotImplementedException();
        }

        public bool GetUserStatus(Guid userId)
        {
            bool? isEnabled = _db.Users
                                 .Where(userInfo => userInfo.Id == userId)
                                 .Select(userInfo => userInfo.Enabled)
                                 .FirstOrDefault();

            if (isEnabled == null)
            {
                throw new InvalidOperationException($"User {userId} not defined");
            }

            return isEnabled.Value;
        }

        public void ChangeUserEnabledStatus(OCommand command, Guid userId, bool enabled)
        {
            var user = _db.Users.Where(userInfo
                => userInfo.Id == userId).FirstOrDefault();
            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not defined");
            }
            user.Enabled = enabled;
            user.SetUpdate<ChangeProps>(command);
            _db.UserHistory.Add(new DALUserHistory() { UserId = userId, CommandId = command.Id, CommandTime = command.Time });
            _db.SaveChanges();
        }

        public void CreatePermissionAdminDefination(OCommand command, PermissionAdminInfo permissionAdminInfo)
        {
            // TODO implement CreatePermissionAdminDefination
            throw new NotImplementedException();
        }

        public void UpdatePermssionAdminDefination(OCommand command, PermissionAdminInfo permissionAdminInfo)
        {
            // TODO implement UpdatePermssionAdminDefination
            throw new NotImplementedException();
        }

        public void SetSystemId(OCommand command, Guid systemId)
        {
            // TODO implement SetSystemId
            throw new NotImplementedException();
        }

        [OViewFunction]
        public UserInfo? GetUser(string userName) => GetUserInfo(userName: userName, false);

        [OViewFunction]
        public UserInfo? GetUserById(Guid userId) => GetUserInfo(userId, false);

        [OViewFunction(permissions: new string[] { CoreModule.PERMISSION_GET_USER })]
        public PagedList<UserInfo> GetUsers(int index, int count, UserInfoFilter? filter = null)
        {
            var queryable = _db.Users.AsQueryable();

            if (filter != null)
            {
                if (filter.Enabled is bool enabled)
                {
                    queryable = queryable.Where(userInfo => userInfo.Enabled == enabled);
                }
            }

            queryable = queryable.Include(userInfo => userInfo.Roles);

            var size = queryable.Count();

            var slicedUserInfoList = queryable
                .OrderBy(userInfo => userInfo.UserName)
                .Skip(index).Take(count).ToList();

            return new PagedList<UserInfo>
            {
                List = slicedUserInfoList.Select(userInfo => new UserInfo(userInfo)
                {
                    PasswordHash = null,
                    Roles = userInfo.Roles.OrderBy(userRole => userRole.Order).Select(userRole => userRole.RoleId).ToList()
                }).ToList(),
                Count = size
            };
        }

        public bool IsUserNameExist(string userName, Guid Id)
        {
            var users = _db.Users.AsQueryable().Where(x => x.UserName.Equals(userName) && x.Id != Id).ToList();
            if (users.Any())
                return true;
            return false;
        }

        [OViewFunction(permissions: new string[] { CoreModule.PERMISSION_GET_USER })]
        public PagedList<UserInfo> SearchUsers(string query, int index, int count, bool? enabled = null)
        {
            var matchingUsers = _db.Users
                .Include(userInfo => userInfo.Roles)
                .Where(userInfo =>
                    (enabled == null || userInfo.Enabled == enabled.Value) &&
                    EF.Functions.ILike(userInfo.UserName, $"%{query}%") ||
                    EF.Functions.ILike(userInfo.FullName, $"%{query}%") ||
                    EF.Functions.ILike(userInfo.Email, $"%{query}%") ||
                    EF.Functions.ILike(userInfo.PhoneNo, $"%{query}%"));

            var size = matchingUsers.Count();

            var slicedUserInfoList = matchingUsers
                .OrderBy(userInfo => userInfo.UserName)
                .Skip(index).Take(count).ToList();

            return new PagedList<UserInfo>
            {
                List = slicedUserInfoList.Select(userInfo => new UserInfo(userInfo)
                {
                    PasswordHash = null,
                    Roles = userInfo.Roles.OrderBy(userRole => userRole.Order).Select(userRole => userRole.RoleId).ToList()
                }).ToList(),
                Count = size
            };
        }

        [OViewFunction]
        public IList<Role> GetUserRoles(Guid id)
        {
            return _db.Roles
                  .Join(_db.UserRoles.Where(userRole => userRole.UserId == id),
                        role => role.Id, userRole => userRole.RoleId, (role, userRole) =>
                        new { a = role, b = userRole })
                  .OrderBy(x => x.b.Order)
                  .Select(x => new Role(x.a)
                  {
                      Permissions = x.a.Permissions.Select(p => p.PermissionId).ToList()
                  })
                  .ToList();
        }

        [OViewFunction(permissions: new string[] { CoreModule.PERMISSION_GET_ROLES })]
        public PagedList<Role> GetAllRoles(int? index, int? count, string? query = null)
        {
            var rolePermissionsList = _db.Roles.AsNoTracking();

            if (!string.IsNullOrEmpty(query))
            {
                rolePermissionsList = rolePermissionsList.Where(x =>
                    EF.Functions.Like(x.Key.ToLower(), $"%{query.ToLower()}%") || EF.Functions.Like(x.RoleName.ToLower(), $"%{query.ToLower()}%") || EF.Functions.Like(x.Description.ToLower(), $"%{query.ToLower()}%"));
            }

            rolePermissionsList = rolePermissionsList.Include(role => role.Permissions);

            int size = rolePermissionsList.Count();
            IQueryable<DALRole> roleList;
            if (index != null)
            {
                roleList = rolePermissionsList
                .OrderBy(role => role.Key)
#pragma warning disable CS8629 // Nullable value type may be null.
                .Skip(index.Value).Take(count.Value);
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            else
                roleList = rolePermissionsList
                .OrderBy(role => role.Key);

            return new PagedList<Role>
            {
                Count = size,
                List = roleList
               .Select(role => new Role(role)
               {
                   Permissions = role.Permissions
                   .OrderBy(rolePermission => rolePermission.Order)
                   .Select(rolePermission => rolePermission.PermissionId).ToList()
               }).ToList()
            };
        }

        public void UpdateSystemInformation(OCommand command, TransactionSystemInformation sysInfo, bool insert)
        {
            if (insert)
            {
                sysInfo.SetCreate<ChangeProps>(command);
                _db.TransactionSystemInformation.Add(new DALTransactionSystemInformation(sysInfo));
            }
            else
            {
                var existing = _db.TransactionSystemInformation.FirstOrDefault();
                if (existing != null)
                {
                    existing.HeadTranId = sysInfo.HeadTranId;
                    existing.SystemId = sysInfo.SystemId;
                    existing.SetUpdate<ChangeProps>(command);
                }
            }
            _db.SaveChanges();
        }

        [OViewFunction]
        public List<Permission> GetAllPermissions()
        {
            return _db.Permissions
                        .OrderBy(permission => permission.PermissionKey).AsEnumerable()
                        .Select(permission => new Permission(permission))
                        .ToList();
        }

        [OViewFunction]
        public List<Permission> GetAllPermissionsByModule(string module)
        {
            return _db.Permissions.Where(x => x.ModuleName.ToLower().Contains(module.ToLower()))
                        .OrderBy(permission => permission.PermissionKey).AsEnumerable()
                        .Select(permission => new Permission(permission))
                        .ToList();
        }

        [OViewFunction]
        public PagedList<Permission> GetPermissionsByRoleId(Guid roleId, int pageIndex, int pageSize)
        {
            var role = _db.Roles.Include(r => r.Permissions)
                                    .ThenInclude(rp => rp.Permission)
                                .FirstOrDefault(r => r.Id == roleId)
                                    ?? throw new InvalidOperationException($"Role with ID '{roleId}' does not exist.");

            var permissionsQuery = role.Permissions.AsQueryable();

            var totalPermissionsCount = permissionsQuery.Count();

            var permissions = permissionsQuery
                                .OrderBy(rp => rp.Order)
                                .Skip(pageIndex * pageSize)
                                .Take(pageSize)
                                .Select(rp => new Permission(rp.Permission))
                                .ToList();

            return new PagedList<Permission>
            {
                List = permissions,
                Count = totalPermissionsCount
            };
        }


        public void DeleteUser(OCommand command, Guid userId)
        {
            if (_db.Commands.Where(command => command.UserId == userId).Any())
            {
                throw new InvalidOperationException("The user can't be deleted because he/she has performed transactions");
            }
            _db.UserRoles.RemoveRange(_db.UserRoles.Where(x => x.UserId == userId));

            var userInfo = _db.Users.Where(userInfo => userInfo.Id == userId).First();

            if (userInfo == null)
            {
                throw new InvalidOperationException($"User {userId} not defined");
            }

            _db.Users.Remove(userInfo);
            _db.UserHistory.Add(new DALUserHistory
            {
                CommandId = command.Id,
                UserId = userId,
                CommandTime = command.Time,
            });
            _db.SaveChanges();
        }

        [OViewFunction]
        public bool IsPermitted(Guid userId, Guid permissionId)
        {
            var permission = _db.Permissions.Where(x => x.Id == permissionId).FirstOrDefault();
            if (permission == null)
            {
                throw new InvalidOperationException($"Permssion id {permissionId} not defined");
            }
            return IsPermitted(userId, permissionKey: permission.PermissionKey);
        }

        public bool IsPermitted(Guid userId, string permissionKey)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty");

            var permission = _db.Permissions.Where(permission => permission.PermissionKey == permissionKey).FirstOrDefault();
            if (permission == null)
                throw new InvalidOperationException($"Permission key {permissionKey} not defined");

            if (userId == GetRootUser()?.Id)
                return true;

            return _db.UserRoles
                .Where(userRole => userRole.UserId == userId) //select the roles of the user
                .Join(_db.PermissionRoles, a => a.RoleId, b => b.RoleId, (a, b) => b) //join with permssion roles table
                .Where(rolePermission => rolePermission.PermissionId == permission.Id).Any();//filter by the required permssion
        }

        public List<Guid> GetUsersWithPermissions(string[] permissionKeys)
        {
            var permissionKeyList = permissionKeys.ToList();

            // Validation: Check if all provided permission keys exist in the database
            var validPermissionIds = _db.Permissions.AsNoTracking()
                                                    .Where(p => permissionKeyList.Contains(p.PermissionKey))
                                                    .Select(p => p.Id)
                                                    .ToList();

            if (validPermissionIds.Count != permissionKeyList.Count)
            {
                var invalidKeys = permissionKeyList.Except(_db.Permissions.AsNoTracking()
                                                                        .Where(p => validPermissionIds.Contains(p.Id))
                                                                        .Select(p => p.PermissionKey))
                                                                        .ToList();

                throw new InvalidOperationException($"Invalid permission keys: {string.Join(", ", invalidKeys)}");
            }

            // Find all roles associated with these permissions
            var rolesWithPermissions = _db.PermissionRoles.AsNoTracking()
                                                          .Where(pr => validPermissionIds.Contains(pr.PermissionId))
                                                          .Select(pr => pr.RoleId)
                                                          .Distinct()
                                                          .ToList();

            // Find all user IDs associated with these roles
            var userIdsWithRoles = _db.UserRoles.AsNoTracking()
                                                .Where(ur => rolesWithPermissions.Contains(ur.RoleId))
                                                .Select(ur => ur.UserId)
                                                .Distinct()
                                                .ToList();

            return userIdsWithRoles;
        }



        [OViewFunction]
        public bool IsUserPermittedByKey(Guid userId, string permssionKey)
        {
            var permission = _db.Permissions.Where(permission => permission.PermissionKey == permssionKey).FirstOrDefault();
            if (permission == null)
            {
                throw new InvalidOperationException($"Permssion key {permssionKey} not defined");
            }
            return _db.UserRoles
                .Where(userRole => userRole.UserId == userId) //select the roles of the user
                .Join(_db.PermissionRoles, a => a.RoleId, b => b.RoleId, (a, b) => b) //join with permssion roles table
                .Where(rolePermission => rolePermission.PermissionId == permission.Id).Any();//filter by the required permssion
        }

        [OViewFunction("IsPermittedAll")]
        public bool IsPermitted(Guid userId, string[] permissionKeys, out string[] notGrantedPermissions)
        {
            var permissionKeyList = permissionKeys.ToList();

            if (userId == Guid.Empty)
            {
                notGrantedPermissions = Array.Empty<string>();
                return false;
            }

            if (permissionKeyList.Count == 0)
            {
                notGrantedPermissions = Array.Empty<string>();
                return true;
            }

            var permissions = _db.Permissions.Where(x => permissionKeyList.Contains(x.PermissionKey)).ToList();
            if (permissions.Count != permissionKeyList.Count)
            {
                var notFoundKeys = permissionKeyList.Except(permissions.Select(p => p.PermissionKey));
                throw new InvalidOperationException($"Permission key(s) {string.Join(", ", notFoundKeys)} not found");
            }

            if (GetRootUser() is { } rootUser && rootUser.Id == userId)
            {
                notGrantedPermissions = Array.Empty<string>();
                return true;
            }

            var userPermissionIds = _db.UserRoles
                .Where(x => x.UserId == userId) // select the roles of the user
                .Join(_db.PermissionRoles, a => a.RoleId, b => b.RoleId, (a, b) => b) // join with permission roles table
                .Select(x => x.PermissionId)
                .Distinct()
                .ToList();

            bool isPermitted = permissions.All(p => userPermissionIds.Contains(p.Id));

            if (!isPermitted)
            {
                notGrantedPermissions = permissions.Where(p => !userPermissionIds.Contains(p.Id)).Select(p => p.PermissionKey).ToArray();
            }
            else
            {
                notGrantedPermissions = Array.Empty<string>();
            }

            return isPermitted;
        }

        [OViewFunction("IsPermittedAny")]
        public bool IsPermittedAny(Guid userId, params string[] permissionKeys)
        {
            var permissionKeyList = permissionKeys.ToList();
            var permissions = _db.Permissions.Where(x => permissionKeyList.Contains(x.PermissionKey)).ToList();
            if (permissions.Count == 0)
            {
                var notFoundKeys = permissionKeyList.Except(permissions.Select(p => p.PermissionKey));
                throw new InvalidOperationException($"Permission key(s) {string.Join(", ", notFoundKeys)} not found");
            }

            if (GetRootUser() is { } rootUser && rootUser.Id == userId)
            {
                return true;
            }

            var userPermissionIds = _db.UserRoles
                .Where(x => x.UserId == userId) // select the roles of the user
                .Join(_db.PermissionRoles, a => a.RoleId, b => b.RoleId, (a, b) => b) // join with permission roles table
                .Select(x => x.PermissionId)
                .Distinct()
                .ToList();

            return permissions.Any(p => userPermissionIds.Contains(p.Id));
        }

        [OViewFunction]
        public SerialType? GetSerialType(string key)
        {
            return _db.SerialTypes
                .AsNoTracking()
                .Where(serialType => serialType.Key == key)
                .Select(x => new SerialType(x))
                .FirstOrDefault();
        }

        [OViewFunction("GetSerialTypeById")]
        public SerialType? GetSerialType(Guid id)
        {
            return _db.SerialTypes
                .AsNoTracking()
                .Where(serialType => serialType.Id == id)
                .AsEnumerable()
                .Select(serialType => new SerialType(serialType))
                .FirstOrDefault();
        }

        public void CreateSerialType(OCommand command, SerialType serialType)
        {
            if (GetSerialType(serialType.Key) != null)
            {
                throw new InvalidOperationException($"Serial type key {serialType.Key} already used");
            }

            serialType.SetCreate<ChangeProps>(command);
            _db.SerialTypes.Add(new DALSerialType(serialType));
            _db.SaveChanges();
        }

        public void UpdateSerialType(OCommand command, SerialType serialType)
        {
            var existing = _db.SerialTypes.AsNoTracking()
                               .FirstOrDefault(x => x.Id == serialType.Id)
                           ?? throw new ArgumentException($"Serial type with ID '{serialType.Id}' not found");

            serialType.CopyChangeProps(existing);
            serialType.SetUpdate<ChangeProps>(command);
            _db.SerialTypes.Update(new DALSerialType(serialType));
            _db.SaveChanges();
        }

        public void CreateSerialBatch(OCommand command, SerialBatch serialBatch)
        {
            var serialType = _db.SerialTypes.First(serialType => serialType.Id == serialBatch.SerialTypeId);
            if (serialType == null)
            {
                throw new InvalidOperationException($"Serial type id {serialBatch.SerialTypeId} not valid");
            }
            var batch = _db.SerialBatches.Where(batch => batch.SerialTypeId == serialBatch.SerialTypeId
                                                && batch.FromSerialNo <= serialBatch.FromSerialNo
                                                && batch.ToSerialNo >= serialBatch.ToSerialNo
                                                && batch.Id != serialBatch.Id)
                                                .FirstOrDefault();
            if (batch != null)
            {
                throw new InvalidOperationException($"Serial batch overlaps with {batch.FromSerialNo} to {batch.ToSerialNo}");
            }
            var existing = _db.SerialBatches.FirstOrDefault(sb => sb.Id == serialBatch.Id);
            if (existing == null)
            {
                serialBatch.MaxUsed = serialBatch.FromSerialNo - 1;
                serialBatch.SetCreate<ChangeProps>(command);
                _db.SerialBatches.Add(new DALSerialBatch(serialBatch));
            }
            else
            {
                existing.FromSerialNo = serialBatch.FromSerialNo;
                existing.ToSerialNo = serialBatch.ToSerialNo;
                existing.SetUpdate<ChangeProps>(command);
            }
            _db.SaveChanges();
        }

        public void UpdateSerialBatch(OCommand command, SerialBatch serialBatch)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (serialBatch == null)
                throw new ArgumentNullException(nameof(serialBatch));

            if (serialBatch.FromSerialNo < 0)
                throw new ArgumentException($"The value for '{nameof(SerialBatch.FromSerialNo)}' cannot be negative.",
                    nameof(serialBatch));
            if (serialBatch.ToSerialNo < 0)
                throw new ArgumentException($"The value for '{nameof(SerialBatch.ToSerialNo)}' cannot be negative.",
                    nameof(serialBatch));
            if (serialBatch.FromSerialNo > serialBatch.ToSerialNo)
                throw new InvalidOperationException(
                    $"The value for '{nameof(SerialBatch.FromSerialNo)}' ({serialBatch.FromSerialNo}) cannot be greater than '{nameof(SerialBatch.ToSerialNo)}' ({serialBatch.ToSerialNo}).");
            if (serialBatch.MaxUsed < serialBatch.FromSerialNo - 1)
                throw new ArgumentException(
                    $"The value for '{nameof(SerialBatch.MaxUsed)}' must be at least {serialBatch.FromSerialNo - 1}.",
                    nameof(serialBatch));
            if (serialBatch.MaxUsed >= serialBatch.ToSerialNo)
                throw new ArgumentException(
                    $"The value for '{nameof(SerialBatch.MaxUsed)}' must be less than '{nameof(SerialBatch.ToSerialNo)}' ({serialBatch.ToSerialNo}).",
                    nameof(serialBatch));

            var existing = _db.SerialBatches.FirstOrDefault(sb => sb.Id == serialBatch.Id);
            if (existing == null)
                throw new InvalidOperationException(
                    "The specified serial batch was not found. Please check your batch information.");

            var overlappingBatch = _db.SerialBatches
                .Where(batch => batch.SerialTypeId == existing.SerialTypeId &&
                                batch.Id != serialBatch.Id &&
                                batch.FromSerialNo <= serialBatch.ToSerialNo &&
                                batch.ToSerialNo >= serialBatch.FromSerialNo)
                .FirstOrDefault();
            if (overlappingBatch != null)
                throw new InvalidOperationException(
                    $"The new range ('{nameof(SerialBatch.FromSerialNo)}' {serialBatch.FromSerialNo} to '{nameof(SerialBatch.ToSerialNo)}' {serialBatch.ToSerialNo}) overlaps with an existing batch (from {overlappingBatch.FromSerialNo} to {overlappingBatch.ToSerialNo}).");

            if (serialBatch.ToSerialNo <= existing.MaxUsed)
                throw new InvalidOperationException(
                    $"The value for '{nameof(SerialBatch.ToSerialNo)}' must be greater than the highest issued serial number ({existing.MaxUsed}).");

            if (serialBatch.MaxUsed < existing.MaxUsed)
                throw new InvalidOperationException(
                    $"The new value for '{nameof(SerialBatch.MaxUsed)}' cannot be lower than the current maximum issued serial number ({existing.MaxUsed}).");

            existing.Description = serialBatch.Description;
            existing.FromSerialNo = serialBatch.FromSerialNo;
            existing.ToSerialNo = serialBatch.ToSerialNo;
            existing.MaxUsed = serialBatch.MaxUsed;
            existing.SetUpdate<ChangeProps>(command);

            _db.SaveChanges();
        }

        public void DeleteLastSerialNo(OCommand command, Guid batchId)
        {
            var batch = _db.SerialBatches.FirstOrDefault(b => b.Id == batchId);
            if (batch == null)
            {
                throw new InvalidOperationException($"Serial batch with ID '{batchId}' does not exist.");
            }

            if (batch.MaxUsed < batch.FromSerialNo)
            {
                throw new InvalidOperationException("No serial numbers have been used from this batch yet.");
            }

            var lastSerial = _db.UsedSerials
                .Where(us => us.BatchId == batchId && us.Sn == batch.MaxUsed)
                .SingleOrDefault();

            if (lastSerial == null)
            {
                throw new InvalidOperationException("The last serial number could not be found.");
            }

            batch.MaxUsed--;
            _db.UsedSerials.Remove(lastSerial);

            _db.SaveChanges();
        }


        [OViewFunction]
        public SerialBatch? GetSerialBatchBySerialType(Guid SerialTypeId)
        {
            var batch = _db.SerialBatches
                .Where(serialBatch => serialBatch.SerialTypeId == SerialTypeId)
                .FirstOrDefault();
            return batch == null ? null : new SerialBatch(batch);
        }

        [OViewFunction]
        public SerialNo? GetSerialNo(Guid batchId)
        {
            return _db.UsedSerials
                .Where(serialNo => serialNo.BatchId == batchId)
                .Select(serialNo => new SerialNo(serialNo))
                .FirstOrDefault();
        }

        public SerialNo? GetLastSerialNo(Guid batchId)
        {
            if (!_db.SerialBatches
                    .AsNoTracking()
                    .Any(x => x.Id == batchId))
            {
                throw new InvalidOperationException($"Serial batch with ID '{batchId}' does not exist.");
            }

            return _db.UsedSerials
                .AsNoTracking()
                .Where(serialNo => serialNo.BatchId == batchId)
                .OrderByDescending(serialNo => serialNo.Sn)
                .Select(serialNo => new SerialNo(serialNo))
                .FirstOrDefault();
        }

        [OViewFunction]
        public List<SerialNo> GetSerialNos()
        {
            return _db.UsedSerials
                .AsNoTracking()
                .AsEnumerable()
                .Select(serialNo => new SerialNo(serialNo))
                .ToList();
        }

        [OViewFunction(permissions: new[] { CoreModule.PERMISSION_GET_SERIALS })]
        public List<SerialType> GetSerialTypes()
        {
            return _db
                .SerialTypes
                .AsNoTracking()
                .AsEnumerable()
                .Select(serialType => new SerialType(serialType))
                .ToList();
        }

        [OViewFunction]
        public List<SerialBatch> GetSerialBatches()
        {
            return _db
                .SerialBatches
                .AsNoTracking()
                .AsEnumerable()
                .Select(serialBatch => new SerialBatch(serialBatch))
                .ToList();
        }

        public Guid AddFileReference(OCommand command, ContentReference cref)
        {
            var file = this._systemDatabase.GetFile(cref.FileId);
            if (file == null)
            {
                //TODO: temp bypass for sync
                return Guid.Empty;
                throw new InvalidOperationException($"File id {cref.FileId} is not valid");
            }
            cref.SetCreate<ChangeProps>(command);
            _db.ContentReferences.Add(new DALContentReference(cref));
            _db.SaveChanges();
            return cref.Id;
        }

        public void ReleaseReference(OCommand command, Guid fileId)
        {
            var fileRef = _db.ContentReferences.Where(reference => reference.FileId == fileId).FirstOrDefault();
            if (fileRef == null)
            {
                throw new InvalidOperationException($"File reference id {fileId} is not valid");
            }
            fileRef.SetUpdate<ChangeProps>(command);
            _db.ContentReferences.Remove(fileRef);
            _db.SaveChanges();
        }

        [OViewFunction]
        public SerialBatch? GetDefaultSerialBatch(string typeKey)
        {
            var serialType = _db.SerialTypes.Where(x => x.Key == typeKey).FirstOrDefault();
            if (serialType == null)
            {
                return null;
            }
            var batches = _db.SerialBatches.Where(x => x.SerialTypeId == serialType.Id).Take(2);

            return batches.Count() switch
            {
                0 => null,
                2 => throw new InvalidConfigurationError($"There are multiple serial batches configrued for {typeKey}"),
                _ => batches.Select(serialBatch => new SerialBatch(serialBatch)).First()
            };
        }

        [OViewFunction]
        public SerialBatch? GetSerialBatch(Guid batchId)
        {
            return _db.SerialBatches
                .AsNoTracking()
                .Where(serialBatch => serialBatch.Id == batchId).AsEnumerable()
                .Select(serialBatch => new SerialBatch(serialBatch))
                .FirstOrDefault();
        }

        public void UpdateRole(OCommand command, Role existing)
        {
            existing.SetUpdate<ChangeProps>(command);
            var existingRole = _db.Roles.Include(role => role.Permissions).FirstOrDefault(role => role.Id == existing.Id);
            if (existingRole == null)
            {
                throw new InvalidOperationException($"Role id {existing.Id} is invalid");
            }
            existingRole.RoleName = existing.RoleName;

            _db.PermissionRoles.RemoveRange(existingRole.Permissions);

            if (existing.Permissions != null)
            {
                var order = 1;
                foreach (var permissionId in existing.Permissions)
                    _db.PermissionRoles.Add(new DALRolePermission
                    {
                        Order = order++,
                        PermissionId = permissionId,
                        RoleId = existingRole.Id
                    });
            }
            _db.SaveChanges();
        }

        [OViewFunction]
        public OTransaction GetTransaction(Guid tranId)
        {
            var transaction = _db.Transactions.Where(x => x.Id == tranId)
                .Select(x => new OTransaction(x))
                .FirstOrDefault();

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction id {tranId} is invalid");
            }

            return transaction;
        }

        [OViewFunction]
        public PagedList<OTransaction> GetTransactions(long afterSeqNo, int count)
        {
            var transactions = _db.Transactions
                .AsNoTracking()
                .Where(t => t.SeqNo > afterSeqNo)
                .OrderBy(t => t.SeqNo)
                .Take(count).Select(t => new OTransaction(t))
                .ToList();

            var totalTransactionCount = _db.Transactions.AsNoTracking().Count();

            return new PagedList<OTransaction>
            {
                List = transactions,
                Count = totalTransactionCount
            };
        }

        [OViewFunction]
        public OCommand GetCommand(Guid commandId)
        {
            var command = _db.Commands.Where(command => command.Id == commandId)
                .Select(command => new OCommand(command))
                .FirstOrDefault();

            if (command == null)
            {
                throw new InvalidOperationException($"Command id {commandId} is invalid");
            }

            return command;
        }

        [OViewFunction]
        public IList<OCommand> GetCommandsOf(Guid tranId)
        {
            var transaction = GetTransaction(tranId);

            return _db.Commands.Where(command => command.TranId == transaction.Id).OrderBy(commad => commad.SeqNo)
                        .AsEnumerable()
                        .Select(commad => new OCommand(commad))
                        .ToList();
        }

        [OViewFunction]
        public IList<OCommand> GetCommandsByDataType(Guid tranId, List<Guid> dataTypeIds)
        {
            var transaction = GetTransaction(tranId);

            return _db.Commands
                .Where(command => command.TranId == transaction.Id && dataTypeIds.Contains(command.DataTypeID))
                .OrderBy(command => command.SeqNo)
                .AsEnumerable()
                .Select(command => new OCommand(command))
                .ToList();
        }

        [OViewFunction]
        public PagedList<OTransaction> GetTransactionsByDataType(long afterSeqNo, int count, List<Guid> dataTypeIds)
        {
            var transactionsQuery = _db.Transactions
                .AsNoTracking()
                .Where(t => t.Commands.Any(c => c.TranId == t.Id && dataTypeIds.Contains(c.DataTypeID)));

            var totalTransactionCount = transactionsQuery.Count();

            var transactions = transactionsQuery
                .Where(t => t.SeqNo > afterSeqNo)
                .OrderBy(t => t.SeqNo)
                .Take(count)
                .Select(t => new OTransaction(t))
                .ToList();

            return new PagedList<OTransaction>
            {
                List = transactions,
                Count = totalTransactionCount
            };
        }


        [OViewFunction]
        public OCommand? GetMainCommand(Guid tranId)
        {
            var transaction = GetTransaction(tranId);

            return _db.Commands.Where(command => command.TranId == transaction.Id && command.SeqNo == 1)
                     .Select(commad => new OCommand(commad))
                     .FirstOrDefault();
        }

        [OViewFunction]
        public OJob? GetJob(string jobId)
        {
            return _db.Jobs
                .AsNoTracking()
                .Where(job => job.Id == jobId)
                .Select(job => new OJob(job))
                .FirstOrDefault();
        }

        public void ChangePassword(OCommand command, Guid userId, byte[] passwordHash)
        {
            var existing = _db.Users.FirstOrDefault(x => x.Id == userId);
            if (existing == null)
            {
                throw new InvalidOperationException($"User {userId} not defined");
            }
            existing.PasswordHash = passwordHash;
            existing.SetUpdate<ChangeProps>(command);
            _db.Users.Update(existing);
            _db.SaveChanges();
        }

        public void RecordFailedLogin(Guid userId, int maxFailedAttempts, int lockoutMinutes, long now)
        {
            var lockoutUntil = now + (long)lockoutMinutes * 60 * 1000;
            _db.Database.ExecuteSqlRaw(
                """
                UPDATE core.user_info
                SET failed_login_count = failed_login_count + 1,
                    lockout_until = CASE
                        WHEN failed_login_count + 1 >= {1} THEN {2}
                        ELSE lockout_until
                    END
                WHERE id = {0}
                """,
                userId,
                maxFailedAttempts,
                lockoutUntil);
        }

        public void ResetLoginFailures(Guid userId)
        {
            _db.Database.ExecuteSqlRaw(
                """
                UPDATE core.user_info
                SET failed_login_count = 0,
                    lockout_until = NULL
                WHERE id = {0}
                """,
                userId);
        }

        public void UpdatePasswordHash(Guid userId, byte[] passwordHash)
        {
            _db.Database.ExecuteSqlRaw(
                """
                UPDATE core.user_info
                SET password_hash = @passwordHash
                WHERE id = @userId
                """,
                new NpgsqlParameter("passwordHash", NpgsqlDbType.Bytea) { Value = passwordHash },
                new NpgsqlParameter("userId", NpgsqlDbType.Uuid) { Value = userId });
        }

        [OViewFunction]
        public OrganizationData? GetOrganizationData()
        {

            return _db.Organizations.AsNoTracking()
                                    .Select(organizationData => new OrganizationData(organizationData))
                                    .FirstOrDefault();
        }

        public List<OrganizationData> GetAllOrganizationData()
        {
            return _db.Organizations.AsNoTracking()
                                    .AsEnumerable()
                                    .Select(organizationData => new OrganizationData(organizationData))
                                    .ToList();
        }

        public void SetOrganzationData(OCommand command, OrganizationData orgDat)
        {
            if (_db.Organizations.Any())
            {
                var existing = _db.Organizations.AsNoTracking().First();
                foreach (var fileId in new[] { existing.LogoFileId, existing.LeftLetterHeadingLogoFileId, existing.RightLetterHeadingLogoFileId })
                {
                    if (fileId != null)
                        this.ReleaseReference(command, fileId.Value);
                }
                orgDat.Id = existing.Id;
                orgDat.SetUpdate<ChangeProps>(command);
                orgDat.TransferCreate(existing);
                _db.Organizations.Update(new DALOrganizationData(orgDat));

                foreach (var fileId in new[] { orgDat.LogoFileId, orgDat.LeftLetterHeadingLogoFileId, orgDat.RightLetterHeadingLogoFileId })
                {
                    if (fileId != null)
                    {
                        this.AddFileReference(command, new ContentReference
                        {
                            RefText = $"Organization logo file",
                            FileId = fileId.Value
                        }.SetCreate<ContentReference>(command));
                    }
                }
            }
            else
            {
                orgDat.SetCreate<ChangeProps>(command);
                foreach (var fileId in new[] { orgDat.LogoFileId, orgDat.LeftLetterHeadingLogoFileId, orgDat.RightLetterHeadingLogoFileId })
                {
                    if (fileId != null)
                    {
                        this.AddFileReference(command, new ContentReference
                        {
                            RefText = $"Organization logo file",
                            FileId = fileId.Value
                        }.SetCreate<ContentReference>(command));
                    }
                }
                _db.Organizations.Add(new DALOrganizationData(orgDat));
            }
            _db.SaveChanges();
        }
        public class CommandType
        {
            public Guid TypeId;
            public string Key = string.Empty;
            public string TypeName = string.Empty;
        }

        [OViewFunction]
        public IList<CommandType> GetAllCommandTypes()
        {
            return OTransactionService.GetAllCommandTypes().Select(x => new CommandType
            {
                Key = x.Key,
                TypeName = x.TypeName,
                TypeId = x.TypeId
            }).ToList();
        }

        public class JobType
        {
            public Guid TypeId;
            public string Key = string.Empty;
            public string TypeName = string.Empty;
        }

        [OViewFunction]
        public IList<JobType> GetAllJobTypes()
        {
            return OJobService.GetAllJobTypes().Select(x => new JobType
            {
                Key = x.Key,
                TypeName = x.TypeName,
                TypeId = x.TypeId
            }).ToList();
        }

        public void Dispose()
        {
            _dbTransaction?.Dispose();
            _db.Dispose();
            _systemDatabase.Dispose();
        }
    }
}