namespace orch.core.model
{
    public abstract class HistoryProps
    {
        public Guid CommandId { get; set; }
        public long CommandTime { get; set; }
        public Guid? CommandUserId { get; set; }
        public void CopyFrom(OCommand command)
        {
            CommandId = command.Id;
            CommandTime = command.Time;
            CommandUserId = command.UserId;
        }
    }
#nullable enable
    public abstract class AccessTokenProps
    {
        public Guid Token { get; set; }
        public Guid UserId { get; set; }
        public long CreatedTime { get; set; }
        public long? ExpiryTime { get; set; }
        public long LastUsed { get; set; }
        public string? AuthMethod { get; set; }
        public string? ClientInfoRaw { get; set; }
        public string? ClientInfoJson { get; set; }
        public string? ClientInfoHash { get; set; }
        public string? BrowserFingerprintHash { get; set; }
        public string? NetworkFingerprintHash { get; set; }
        public string? CreatedIp { get; set; }
        public string? LastSeenIp { get; set; }
        public string? XForwardedFor { get; set; }
        public string? ForwardedHeader { get; set; }
        public string? UserAgent { get; set; }
        public string? AcceptLanguage { get; set; }
        public string? Origin { get; set; }
        public string? Referer { get; set; }
        public string? ServerRequestId { get; set; }
    }
#nullable disable

    public abstract class ContentFileProps
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long CreateTime { get; set; }
    }

    public class ContentReferenceProps : ChangeProps
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public string RefText { get; set; }
    }
    public abstract class PermissionProps : ChangeProps
    {
        public const string ROOT_PERMISSION = "SYSTEM_ROOT";
        public const string PERMISSION_ADMIN_PREFIX = "PERMSSION_ADMIN_";
        public const string WILD_CARD_CHAR = "*";
        public const string PERMISSOIN_KEY_PATTERN = "[a-zA-Z0-9_-]{1,}";
        public Guid Id { get; set; }
        public Guid TranId { get; set; }
        public String PermissionName { get; set; }
        public String PermissionKey { get; set; }
        public string ModuleName { get; set; }

        internal static bool ValidatePermissionKey(string key)
        {
            var match = System.Text.RegularExpressions.Regex.Match(key, Permission.PERMISSOIN_KEY_PATTERN);
            return match.Success && match.Length == key.Length;
        }
    }
    public abstract class RoleProps : ChangeProps
    {
        public const string ROLE_FORMAT = "[a-zA-Z0-9_\\-]{1,}";
        public Guid Id { get; set; }
        public String Key { get; set; }
        public String RoleName { get; set; }
        public string Description { get; set; }

        internal static bool ValidateRoleName(string roleName)
        {
            return !string.IsNullOrEmpty(roleName);
        }
    }
    public class UserInfoProps : ChangeProps
    {
        public static readonly string USER_NAME_ROOT = "root";
        public static readonly string USER_NAME_SYSTEM = "system";

        public static readonly string PASSWORD_PATTERN = @"^[^\s]{8,}$";

        public Guid Id { get; set; }
        public long Time { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PublicKey { get; set; }
        public bool Enabled { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ReaderId { get; set; }
        public int FailedLoginCount { get; set; }
        public long? LockoutUntil { get; set; }

    }
    public abstract class UserHistoryProps : HistoryProps
    {
        public Guid UserId { get; set; }
    }
    public abstract class OCommandProps
    {
        public Guid Id { get; set; }
        public Guid TranId { get; set; }

        public int SeqNo { get; set; }
        public int? ParentSeqNo { get; set; }
        public long Time { get; set; }
        public Guid? UserId { get; set; }
        public String TextSummary { get; set; }
        public Guid DataTypeID { get; set; }
        public int FormatVersion { get; set; }
        public String TextData { get; set; }
        public bool MainCommand { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is OCommandProps other)
            {
                return Id == other.Id &&
                       TranId == other.TranId &&
                       SeqNo == other.SeqNo &&
                       ParentSeqNo == other.ParentSeqNo &&
                       Time == other.Time &&
                       UserId == other.UserId &&
                       TextSummary == other.TextSummary &&
                       DataTypeID == other.DataTypeID &&
                       FormatVersion == other.FormatVersion &&
                       TextData == other.TextData &&
                       MainCommand == other.MainCommand;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TranId, SeqNo, ParentSeqNo, UserId, DataTypeID, FormatVersion, MainCommand);
        }
    }

    public abstract class OJobProps
    {
        public string Id { get; set; }
        public long Time { get; set; }
        public Guid UserId { get; set; }
        public string TextSummary { get; internal set; }
        public Guid DataTypeID { get; set; }
        public string TextData { get; set; }
        public Guid? SystemID { get; set; }
    }

    public abstract class OTransactionProps
    {
        public Guid Id { get; set; }
        public Guid? PrevId { get; set; }
        public long SeqNo { get; set; }
        public String TextSummary { get; set; }
        public long Time { get; set; }
        public Guid? UserId { get; set; }
        public Guid? SystemID { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is OTransactionProps other)
            {
                return Id == other.Id &&
                       PrevId == other.PrevId &&
                       SeqNo == other.SeqNo &&
                       TextSummary == other.TextSummary &&
                       Time == other.Time &&
                       UserId == other.UserId &&
                       SystemID == other.SystemID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, PrevId, SeqNo, TextSummary, Time, UserId, SystemID);
        }
    }
}
