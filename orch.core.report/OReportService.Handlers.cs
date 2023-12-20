using orch.report.Handler;
using System.Reflection;

namespace orch.report
{
    public partial class OReportService
    {
        private static readonly Dictionary<string, ReportTypeInfo> _reportTypes = new();
        private static readonly Dictionary<string, ReportHandlerInfo> _reportHandlerInfos = new();
        static readonly Dictionary<string, List<ReportTypeInfo>> s_reportTypesByAssembly = new();

        public static IList<string> GetAllReportTypeKeys()
        {
            return _reportTypes.Values.Select(x => x.Key).ToList();
        }

        public static void LoadReportTypesFromAssembly(Assembly assembly)
        {
            lock (_reportTypes)
            {

                var assemblyName = assembly.GetName().Name;
                if (s_reportTypesByAssembly.ContainsKey(assemblyName))
                {
                    throw new InvalidOperationException($"Transaction Types for {assemblyName} have already registered.");
                }

                var types = assembly.GetTypes();
                s_reportTypesByAssembly[assemblyName] = new List<ReportTypeInfo>();

                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<ReportTypeAttribute>(false);
                    if (attribute is null || attribute.Info is null) continue;
                    if (_reportTypes.TryGetValue(attribute.Info.Key, out var existingReportTypeInfo))
                    {
                        throw new InvalidOperationException($"Report type key {attribute.Info.Key} is already defined for type {existingReportTypeInfo.TypeName}." +
                            $" Cannot redefine as {type.FullName}.");
                    }
                    _reportTypes.Add(attribute.Info.Key, attribute.Info);

                    attribute.Info.TypeName ??= type.FullName;

                    attribute.Info.Type = type;

                    if (attribute.Handler is null) continue;

                    if (attribute.Handler.GetInterface(typeof(IReportHandler).FullName ?? string.Empty) is null) continue;

                    var constructors = attribute.Handler.GetConstructors();

                    if (constructors.Length is 0)
                    {
                        throw new InvalidOperationException($"Report Handler {attribute.Handler} doesn't have a constructor");
                    }
                    if (constructors.Length is not 1)
                    {
                        throw new InvalidOperationException($"Found multiple constructors for {attribute.Handler}");
                    }

                    _reportHandlerInfos.Add(attribute.Info.Key, new ReportHandlerInfo
                    {
                        HandlerType = attribute.Handler,
                        ConstructorParameters = constructors.First().GetParameters().Select(paramInfo => paramInfo.ParameterType).ToArray(),
                        SupportedFormats = GetSupportedFormats(attribute.Handler)
                    });

                    s_reportTypesByAssembly[assemblyName].Add(attribute.Info);

                }
            }
        }

        public IReportHandler? GetHandler(string key)
        {
            if (_reportHandlerInfos.TryGetValue(key, out var handlerInfo) && handlerInfo.HandlerType is not null)
            {
                var pars = handlerInfo.ConstructorParameters?.Select(paramType =>
                {
                    if (paramType.IsAssignableFrom(typeof(OReportService)))
                    {
                        return this;
                    }
                    return _services.GetService(paramType);
                }).ToArray();

                return Activator.CreateInstance(handlerInfo.HandlerType, pars) as IReportHandler;
            }
            return null;
        }

        public static ReportTypeInfo? GetTypeInfoByKey(string key)
            => _reportTypes.GetValueOrDefault(key);

        public static ReportHandlerInfo? GetHandlerInfoByKey(string key)
            => _reportHandlerInfos.GetValueOrDefault(key);

        public static IList<string> GetAllAssemblyNames()
            => s_reportTypesByAssembly.Keys.ToList();

        public static IList<ReportTypeInfo> GetReportTypesByAssembly(string assemblyName)
            => s_reportTypesByAssembly.ContainsKey(assemblyName)
                ? s_reportTypesByAssembly[assemblyName]
                : throw new ArgumentException($"No types found for assembly {assemblyName}");

        /// <summary>
        /// Returns a list of report formats that are supported by the specified report handler type.
        /// Utilizes reflection to determine whether the report handler type implements a suitable format method.
        /// </summary>
        /// <param name="handler">The report handler type to check for supported formats.</param>
        /// <returns>A list of report formats that are supported by the specified report handler type.</returns>
        private static List<ReportFormat> GetSupportedFormats(Type handler)
        {
            var formats = new List<ReportFormat>();

            if (handler.IsAssignableTo(typeof(IReportHandler)))
            {
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                var generateCSVMethod = handler.GetMethod(nameof(IReportHandler.GenerateCSV), flags);
                if (generateCSVMethod is not null)
                {
                    formats.Add(ReportFormat.CSV);
                }
                var generatePDFMethod = handler.GetMethod(nameof(IReportHandler.GeneratePDF), flags);
                if (generatePDFMethod is not null)
                {
                    formats.Add(ReportFormat.PDF);
                }
            }
            return formats;
        }

        public static void Reset()
        {
            lock (_reportTypes)
            {
                _reportTypes.Clear();
            }

            lock (_reportHandlerInfos)
            {
                _reportHandlerInfos.Clear();
            }

            lock (s_reportTypesByAssembly)
            {
                s_reportTypesByAssembly.Clear();
            }
        }
    }
}