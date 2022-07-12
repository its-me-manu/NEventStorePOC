using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace NEventStorePOC.Framework
{
    public class Command
    {
        public Guid Id { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        /// <summary>
        /// This property will be used for optimistic concurrency checks.  It should always be one greater than the last saved command
        /// </summary>
        public long Version { get; set; }
        public string StationNumber { get; set; }
        public TimeSpan ActiveTime { get; set; }
        public int KeystrokeCount { get; set; }
        public int MouseClickCount { get; set; }
        public DateTime ScheduledTime { get; set; }
        protected static CommandExtensions Extensions { get; set; }

        public AccountDetails Account { get; set; }

        static Command()
        {
            Extensions = new CommandExtensions();
        }

        public string ConvertToJson()
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var serializer = new Newtonsoft.Json.JsonSerializer { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, this);
            return sb.ToString();
        }

    }

    public abstract class Command<T> : Command
    {
        public T Materialize(T materializedObject)
        {
            var result = OnMaterialize(materializedObject);
            var extension = (CommandMaterializationExtension<T>)Extensions.Extensions.SingleOrDefault(x => x.MaterializedObjectType == typeof(T));
            extension?.AfterMaterialization(this, result);
            return result;
        }

        protected abstract T OnMaterialize(T materializedObject);
    }

    public class AccountDetails
    {
        public string VDN { get; set; }

        public string ShortAnswerScript { get; set; }

        public string AnswerScriptDetails { get; set; }

        public string ExternalFile { get; set; }

        public string AnswerScriptDetailType { get; set; }

        public string CID { get; set; }

        public string SID { get; set; }

        public string PID { get; set; }

        public string AnswerScript { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public long? StateProvinceId { get; set; }
        public StateProvince StateProvince { get; set; }

        public long? TimeZoneId { get; set; }
        public TimeZone TimeZone { get; set; }

        public long? StatusId { get; set; }

        public AccountStatus Status { get; set; }

        public bool IsLive { get; set; }

        public long? AccountServiceId { get; set; }
        public AccountService AccountService { get; set; }

        public long? RelationshipManagerId { get; set; }
        public RelationshipManager RelationshipManager { get; set; }
        public bool IsPriorityFinalization { get; set; }
        public bool IsAutoPurgeEnabled { get; set; }
        public short AutoPurgeDeadlineInHours { get; set; }
        public DateTimeOffset? AutoPurgeEffectiveDate { get; set; }
        public FtpSettings FtpSettings { get; set; }
        public bool IsOutboundCallFeatureEnabled { get; set; }
        public bool SendReportsToShareFile { get; set; }
    }

    public class FtpSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public int Port { get; set; } = 22;
        public string Directory { get; set; } = "/";
        public string PasswordKeyIdentifier { get; set; }
        public byte[] PasswordEncrypted { get; set; }
        public bool? AllowPrivateKeyAuthentication { get; set; }
        public byte[] PassPhraseEncrypted { get; set; }      
        public string PrivateKeyEncryptionIdentifier { get; set; }
        public byte[] PrivateKeyContent { get; set; }
        public string PassPhrase { get; set; }
        public bool IsEnable { get; set; }
        public string HostKeyFingerPrint { get; set; }

    }
    public class RelationshipManager
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Id { get; set; }
    }

    public class AccountService
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class AccountStatus
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class StateProvince
    {
        public int Id { get; set; }
        public long? CountryId { get; set; }
        public Country Country { get; set; }
        public string Abbreviation { get; set; }
        public string Name { get; set; }
    }

    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int? SortOrder { get; set; }
    }

    public class TimeZone
    {
        public int Id { get; set; }
        public static readonly TimeZoneInfo Pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        public string Abbreviation { get; set; }
        public string TimeZoneInfoId { get; set; }
        public int DisplayOrder { get; set; }

    }
}