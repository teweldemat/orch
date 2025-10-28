using FuncScript;
using FuncScript.Core;
using FuncScript.Model;
using Microsoft.Extensions.DependencyInjection;
using orch.core.job;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web;

namespace orch.core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OViewAttribute : Attribute
    {
        public string Name { get; set; }
        public string[] Permissions { get; private set; }

        public OViewAttribute()
        {
        }

        public OViewAttribute(string name, string[] permissions = null)
        {
            Name = name;
            Permissions = permissions;
        }

        public OViewAttribute(string[] permissions)
        {
            Permissions = permissions;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OViewFunctionAttribute : Attribute
    {
        public string Name { get; private set; }
        public string[] Permissions { get; private set; }

        public OViewFunctionAttribute()
        {
        }

        public OViewFunctionAttribute(string name, string[] permissions = null)
        {
            Name = name;
            Permissions = permissions;
        }

        public OViewFunctionAttribute(string[] permissions)
        {
            Permissions = permissions;
        }
    }

    public class OViewUserAttribute : Attribute
    {
    }

    public class ViewFunction
    {
        public Type Service;
        public MethodInfo Method;
        public string Name;
        public string[] Permissions;
        public ParameterInfo UserPar = null;
        public ParameterInfo[] Pars;
        public Dictionary<ParameterInfo, object> DefaultValues = new();
    }

    public class ViewFunctionCaller : FuncScript.Core.IFsFunction
    {
        private OTransactionService _tranService;
        private ViewFunction _func;
        private Guid _userId;

        public ViewFunctionCaller(OTransactionService tranService, ViewFunction func, Guid userId)
        {
            _tranService = tranService;
            _func = func;
            _userId = userId;
        }

        public int MaxParsCount => _func.UserPar == null ? _func.Pars.Length : _func.Pars.Length - 1;

        public CallType CallType => CallType.Prefix;

        public string Symbol => _func.Name;
        public int Precedence { get; }


        public object Evaluate(IFsDataProvider parent, IParameterList pars)
        {
            var serviceType = _func.Service;
            object serviceObject;

            if (_func.Method.IsStatic)
            {
                serviceObject = null;
            }
            else if (serviceType.IsAbstract && serviceType.IsSealed)
            {
                serviceObject = serviceType;
            }
            else
            {
                serviceObject = _tranService.Services.GetRequiredService(serviceType);
            }

            var parVals = new object[_func.Pars.Length];
            var index = 0;

            for (int i = 0; i < parVals.Length; i++)
            {
                ParameterInfo currentPar = _func.Pars[i];

                if (_func.UserPar != null && _func.UserPar.Position == i)
                {
                    parVals[i] = _userId;
                }
                else
                {
                    object parVal = index < pars.Count ? pars.GetParameter(parent, index) : null;
                    Type parType = currentPar.ParameterType;

                    // Use default value if parameter is missing and a default exists
                    if (parVal == null && _func.DefaultValues.TryGetValue(currentPar, out var value))
                    {
                        parVal = value;
                    }

                    if (parVal is KeyValueCollection collection)
                    {
                        parVal = collection.ConvertTo(parType);
                    }


                    if (Nullable.GetUnderlyingType(parType)?.IsEnum == true)
                    {
                        parType = Nullable.GetUnderlyingType(parType);
                    }

                    if (parType.IsEnum && parVal is string parValStr)
                    {
                        parVal = Enum.Parse(parType, parValStr);
                    }

                    if (parVal is FsList list1 && (parType.IsArray || parType.GetInterfaces().Contains(typeof(IList))))
                    {
                        var listData = list1;
                        Type elementType;

                        if (parType.IsArray)
                        {
                            elementType = parType.GetElementType();
                        }
                        else // for IList or List
                        {
                            elementType = parType.GetGenericArguments()[0];
                        }

                        var listType = typeof(List<>).MakeGenericType(elementType);
                        var list = (IList)Activator.CreateInstance(listType);


                        foreach (var x in listData)
                        {
                            if (elementType.IsEnum && x is string xStr)
                            {
                                list.Add(Enum.Parse(elementType, xStr));
                            }
                            else
                            {
                                list.Add(Convert.ChangeType(x, elementType));
                            }
                        }
                        // if type is array, we convert the list to an array
                        if (parType.IsArray)
                        {
                            Array array = Array.CreateInstance(elementType, list.Count);
                            list.CopyTo(array, 0);
                            parVal = array;
                        }
                        else // for IList or List, we leave it as a list
                        {
                            parVal = list;
                        }
                    }

                    parVals[i] = parVal;
                    index++;
                }
            }
            return Engine.NormalizeDataType(_func.Method.Invoke(serviceObject, parVals));
        }


        public string ParName(int index)
        {
            if (_func.UserPar != null && index >= _func.UserPar.Position)
                return _func.Pars[index + 1].Name;
            return _func.Pars[index].Name;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Symbol);
            sb.Append('(');
            int c = this.MaxParsCount;
            for (int i = 0; i < c; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(this.ParName(i));
            }
            sb.Append(')');
            return sb.ToString();
        }
    }

    internal class ServiceFunctionCollection : KeyValueCollection
    {
        private readonly OTransactionService _tranService;
        private readonly Guid _userId;
        private readonly Dictionary<string, ViewFunction> _funcs;
        private readonly Dictionary<string, ViewFunctionCaller> _callers = new();

        public ServiceFunctionCollection(OTransactionService tranService, Guid userId, Dictionary<string, ViewFunction> funcs)
        {
            _tranService = tranService;
            _userId = userId;
            _funcs = funcs;
        }

        public override bool IsDefined(string key)
        {
            return _funcs.ContainsKey(key);
        }

        public override object Get(string key)
        {
            if (_callers.TryGetValue(key, out var f))
            {
                return f;
            }
            if (!_funcs.TryGetValue(key, out var vf))
            {
                throw new InvalidOperationException($"View function {key} not found");
            }

            Authorize(vf);

            var ret = new ViewFunctionCaller(_tranService, vf, _userId);
            _callers[key] = ret;
            return ret;
        }

        public override IFsDataProvider ParentProvider => null;

        private void Authorize(ViewFunction vf)
        {
            if (vf?.Permissions?.Any() == true && !_tranService.IsRootUser(_userId))
            {
                if (!_tranService.Db.IsPermitted(_userId, vf.Permissions, out var notGrantedPermissions))
                {
                    var notGrantedPermissionsStr = string.Join(", ", notGrantedPermissions);
                    throw new UnauthorizedAccessException($"You are not authorized to query: {vf.Name}. Missing permissions: {notGrantedPermissionsStr}");
                }
            }
        }

        public override IList<KeyValuePair<string, object>> GetAll()
        {
            return _funcs.Select(x => KeyValuePair.Create(x.Key, this.Get(x.Key))).ToList();
        }
    }

    public class ViewQueryProvider : IFsDataProvider
    {
        private readonly OTransactionService _tranService;
        private readonly Guid _userId;
        private readonly object _pars;
        private readonly Dictionary<string, ServiceFunctionCollection> _services;
        private DefaultFsDataProvider g = new DefaultFsDataProvider();

        public ViewQueryProvider(OTransactionService tranService, Guid userId, object pars)
        {
            _tranService = tranService;
            _userId = userId;
            _services = new Dictionary<string, ServiceFunctionCollection>();
            _pars = pars;
        }



        public object Get(string name)
        {
            //check from loaded services
            if (_services.TryGetValue(name, out var sc))
                return sc;
            //check from avialable services
            var col = QueryComposer.GetViewFunctionCollection(name);
            if (col != null)
            {
                var newSc = new ServiceFunctionCollection(_tranService, _userId, col);
                _services.Add(name, newSc);
                return newSc;
            }

            if ("pars".Equals(name))
                return _pars;
            return g.Get(name);
        }

        public IFsDataProvider ParentProvider => g;
        public bool IsDefined(string name)
        {
            if (_services.ContainsKey(name))
                return true;

            var col = QueryComposer.GetViewFunctionCollection(name);
            if (col != null)
            {
                return true;
            }
            if ("pars".Equals(name))
                return true;
            return g.IsDefined(name);
        }
    }

    [OView("util")]
    public class ViewUtils
    {
        [OViewFunction]
        public DateTime IntToDate(long dateTime)
        {
            return orch.common.Helpers.LongToTime(dateTime);
        }

        [OViewFunction]
        public long DoubleToMoney(double val)
        {
            return orch.common.Helpers.DoubleToMoney(val);
        }

        [OViewFunction]
        public double MoneyToDouble(long val)
        {
            return orch.common.Helpers.MoneyToDouble(val);
        }

        [OViewFunction]
        public long DateToInt(DateTime date)
        {
            return orch.common.Helpers.TimeToLong(date);
        }

        [OViewFunction]
        public object JFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return null;
            return Engine.FromJson(System.IO.File.ReadAllText(fileName));
        }

        [OViewFunction]
        public CommandTypeInfo GetCommandTypeInfo(String key)
        {
            var ret = OTransactionService.GetTypeIdByKey(key);

            if (ret is null)
                return null;

            return new CommandTypeInfo()
            {
                TypeId = ret.TypeId,
                Key = ret.Key,
                TypeName = ret.TypeName,
            };
        }

        [OViewFunction(name: "GetCommandTypeInfoById")]
        public CommandTypeInfo GetCommandTypeInfo(Guid id)
        {
            var ret = OTransactionService.GetTypeInfoById(id);

            if (ret is null)
                return null;

            return new CommandTypeInfo()
            {
                TypeId = ret.TypeId,
                Key = ret.Key,
                TypeName = ret.TypeName,
            };
        }

        [OViewFunction(name: "GetJobTypeInfoById")]
        public JobTypeInfo GetJobTypeInfo(Guid id)
        {
            var ret = OJobService.GetTypeInfoById(id);

            if (ret is null)
                return null;

            return new JobTypeInfo()
            {
                TypeId = ret.TypeId,
                Key = ret.Key,
                TypeName = ret.TypeName,
            };
        }
    }

    [OView("query")]
    public class QueryComposer
    {
        [OViewFunction]
        public FuncScriptParser.ParseNode ParseFuncScript(string exp)
        {
            var p = new FuncScript.DefaultFsDataProvider();
            var err = new List<FuncScriptParser.SyntaxErrorData>();
            FuncScriptParser.Parse(p, exp, out var node, err);
            return node;
        }

        private int SyntaxHighlight(StringBuilder sb, String exp, int i, FuncScriptParser.ParseNode node)
        {
            if (node == null)
            {
                return i;
            }
            if (node.Childs != null && node.Childs.Count == 1)
            {
                return SyntaxHighlight(sb, exp, i, node.Childs[0]);
            }

            if (node.Childs != null && node.Childs.Count > 0)
            {
                foreach (var ch in node.Childs)
                {
                    i = SyntaxHighlight(sb, exp, i, ch);
                }
                return i;
            }
            else
            {
                if (node.Pos > i)
                {
                    sb.Append("<span style='color:Black'>");
                    var part = exp.Substring(i, node.Pos - i);
                    part = part.Replace(" ", "&nbsp;");
                    sb.Append(part);
                    sb.Append("</span>");
                }
                switch (node.NodeType)
                {
                    case FuncScriptParser.ParseNodeType.Key:
                        sb.Append("<span style='color:DarkGreen'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;

                    case FuncScriptParser.ParseNodeType.Identifier:
                        sb.Append("<span style='color:DarkCyan'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;

                    case FuncScriptParser.ParseNodeType.KeyWord:
                        sb.Append("<span style='color:Blue'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;

                    case FuncScriptParser.ParseNodeType.LiteralInteger:
                        sb.Append("<span style='color:Gray'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;

                    case FuncScriptParser.ParseNodeType.LiteralDouble:
                        sb.Append("<span style='color:Brown'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;

                    case FuncScriptParser.ParseNodeType.LiteralString:
                        sb.Append("<span style='color:Red'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;

                    default:
                        sb.Append("<span style='color:Black'>");
                        sb.Append(HttpUtility.HtmlEncode(exp.Substring(node.Pos, node.Length)));
                        sb.Append("</span>");
                        break;
                }
                return node.Pos + node.Length;
            }
        }

        [OViewFunction]
        public String HighlightFuncScript(string exp)
        {
            var serr = new List<FuncScriptParser.SyntaxErrorData>();
            var p = new DefaultFsDataProvider();
            FuncScriptParser.Parse(p, exp, out var node, serr);
            var sb = new StringBuilder();
            sb.Append("<p>");
            var i = SyntaxHighlight(sb, exp, 0, node);
            if (exp.Length > i)
            {
                sb.Append("<span style='color:Black'>");
                sb.Append(HttpUtility.HtmlEncode(exp.Substring(i, exp.Length - i)));
                sb.Append("</span>");
            }
            sb.Append("</p>");
            return sb.ToString(); ;
        }

        private static Dictionary<String, Dictionary<String, ViewFunction>> s_views = new();

        public static void LoadViews(Assembly a)
        {
            foreach (var type in a.GetTypes())
            {
                var oview = type.GetCustomAttribute<OViewAttribute>();
                if (oview == null)
                    continue;
                var viewName = oview.Name ?? type.Name;
                var viewNameLower = viewName.ToLower();
                Dictionary<string, ViewFunction> funcs;
                if (s_views.ContainsKey(viewNameLower))
                    funcs = s_views[viewNameLower];
                else
                {
                    s_views.Add(viewNameLower, funcs = new Dictionary<string, ViewFunction>());
                }
                foreach (var method in type.GetMethods())
                {
                    var func = method.GetCustomAttribute<OViewFunctionAttribute>();
                    if (func == null)
                        continue;

                    string[] permissions;

                    if (oview.Permissions == null && func.Permissions == null)
                    {
                        permissions = Array.Empty<string>();
                    }
                    else
                    {
                        var oviewPermissions = oview.Permissions ?? Array.Empty<string>();
                        var funcPermissions = func.Permissions ?? Array.Empty<string>();
                        permissions = oviewPermissions.Union(funcPermissions).ToArray();
                    }

                    var funcInfo = new ViewFunction
                    {
                        Name = func.Name ?? method.Name,
                        Permissions = permissions,
                        Method = method,
                        Service = type
                    };
                    var funcNameLower = funcInfo.Name.ToLower();
                    if (funcs.ContainsKey(funcNameLower))
                        throw new InvalidOperationException($"View function {funcNameLower} is already defined");
                    foreach (var par in funcInfo.Pars = method.GetParameters())
                    {
                        var userPar = par.GetCustomAttribute<OViewUserAttribute>();
                        if (userPar != null)
                        {
                            funcInfo.UserPar = par;
                        }
                        else if (par.HasDefaultValue)
                        {
                            funcInfo.DefaultValues[par] = par.DefaultValue;
                        }
                    }
                    funcs.Add(funcNameLower, funcInfo);
                }
            }
        }

        public static ViewFunction GetViewFunction(String service, String function)
        {
            if (!s_views.TryGetValue(service, out var v))
                throw new InvalidOperationException($"View service {service} not found");
            if (!v.TryGetValue(function, out var f))
                throw new InvalidOperationException($"Function {function} not found in service {service}");
            return f;
        }

        public static Dictionary<String, ViewFunction> GetViewFunctionCollection(String service)
        {
            if (!s_views.TryGetValue(service, out var v))
                return null;
            return v;
        }

        public static IList<String> GetViewFunctionCollectionNames()
        {
            return s_views.Keys.ToList();
        }

        public static void Reset()
        {
            s_views.Clear();
        }

        public static object ExecuteQuery(OTransactionService tranService, Guid userId, String query, object parsVal)
        {
            var provider = new ViewQueryProvider(tranService, userId, parsVal);

            var ret = Engine.Evaluate(provider, query);
            return ret;
        }
    }
}