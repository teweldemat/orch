using orch.common;
using orch.core.model;
using orch.core.model.dto;

namespace orch.core
{
    public interface ITransactionDatabase : IDisposable
    {
        bool InTransaction { get; }
        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();

        void EnableReadOnlyMode();

        void DisableReadOnlyMode();

        void AddCommand(OCommand command);

        void AddTransaction(OTransaction tranSet);

        void AddJob(OJob job);

        OCommand GetHeadTransaction();

        TransactionSystemInformation GetCurrentSystemInformation();

        T Deserialize<T>(OCommand command);

        long LastTranSeqNo { get; }

        long Count { get; }

        long CountByDataTypeIds(List<Guid> dataTypeIds);

        UserInfo? GetRootUser();

        UserInfo? GetSystemUser();

        UserInfo GetUserInfo(string userName, bool includePassword = false);

        UserInfo GetUserInfo(Guid agentId, bool includePassword = false);

        UserInfo GetUserByCommandId(Guid id);

        UserInfo GetUserInfoByEmplyeeId(Guid emplyeeId);

        void UpdateUserInfo(UserInfo agent);

        void CreateUser(OCommand command, UserInfo agentInfo);

        void CreateRole(OCommand command, Role role);

        void CreatePermission(OCommand command, Permission permssion);

        void SetUserRole(OCommand command, Guid userId, List<Guid> roles);

        Guid GetSystemUserId();

        void SetRolePermssions(OCommand command, Guid roleId, List<Guid> permssions);

        void ExecuteDml(OCommand command, string dml);

        Role GetRole(Guid role);


        Role GetRole(String key);

        public List<Guid> GetUsersWithRoles(List<Guid> roleIds);

        public PagedList<UserInfo> GetUsers(int index, int count, UserInfoFilter? filter = null);

        public PagedList<UserInfo> SearchUsers(string query, int index, int count, bool? enabled = null);

        List<Guid> GetUserPermissions(Guid agentId);

        Permission GetPermission(Guid permssionId);

        Permission GetPermission(string permKey);

        List<Permission> ExpandPermssions(string permKey);

        List<Guid> GetUserWithPermissions(IList<Guid> userWithPermissions);

        SerialNo UseNextSerialNo(OCommand command, Guid batchId);

        PermissionAdminInfo GetPermissionAdminInfo(string pk);

        bool GetUserStatus(Guid userId);

        void ChangeUserEnabledStatus(OCommand command, Guid userId, bool enabled);

        void CreatePermissionAdminDefination(OCommand command, PermissionAdminInfo permissionAdminInfo);

        void UpdatePermssionAdminDefination(OCommand command, PermissionAdminInfo permissionAdminInfo);

        void SetSystemId(OCommand command, Guid systemId);

        IList<Role> GetUserRoles(Guid id);

        PagedList<Role> GetAllRoles(int? index, int? count, string? query = null);

        void UpdateSystemInformation(OCommand command, TransactionSystemInformation sysInfo, bool insert);

        List<Permission> GetAllPermissions();

        PagedList<Permission> GetPermissionsByRoleId(Guid roleId, int pageIndex, int pageSize);

        List<Permission> GetAllPermissionsByModule(string module);

        void DeleteUser(OCommand command, Guid userId);

        bool IsPermitted(Guid value, Guid permissionId);

        bool IsPermitted(Guid userId, string permissionKey);

        public bool IsPermitted(Guid userId, string[] permissionKeys, out string[] notGrantedPermissions);

        bool IsPermittedAny(Guid userId, string[] permissionKeys);

        List<Guid> GetUsersWithPermissions(string[] permissionKeys);

        SerialType GetSerialType(string key);

        void CreateSerialType(OCommand command, SerialType type);

        void CreateSerialBatch(OCommand command, SerialBatch serialBatch);

        Guid AddFileReference(OCommand command, ContentReference cref);

        void ReleaseReference(OCommand command, Guid fileId);

        SerialBatch GetDefaultSerialBatch(string typeKey);

        SerialBatch GetSerialBatchBySerialType(Guid SerialTypeId);

        SerialType GetSerialType(Guid id);

        SerialNo GetSerialNo(Guid batchId);

        List<SerialType> GetSerialTypes();

        List<SerialBatch> GetSerialBatches();

        List<SerialNo> GetSerialNos();

        SerialBatch GetSerialBatch(Guid batchId);

        void UpdateRole(OCommand command, Role existing);

        OTransaction GetTransaction(Guid tranId);

        PagedList<OTransaction> GetTransactions(long afterSeqNo, int count);

        PagedList<OTransaction> GetTransactionsByDataType(long afterSeqNo, int count, List<Guid> dataTypeIds);

        OCommand GetCommand(Guid commandId);

        IList<OCommand> GetCommandsOf(Guid tranId);

        IList<OCommand> GetCommandsByDataType(Guid tranId, List<Guid> dataTypeIds);

        OCommand GetMainCommand(Guid tranId);

        OJob? GetJob(string jobId);

        void ChangePassword(OCommand command, Guid userId, byte[] passwordHash);

        OrganizationData GetOrganizationData();

        List<OrganizationData> GetAllOrganizationData();

        void SetOrganzationData(OCommand command, OrganizationData orgDat);

    }
}