using funcscript;
using funcscript.core;
using System.Text;

namespace fsstudio
{
    internal class OrchFunction : funcscript.core.IFsFunction
    {
        MainWindow _parent;
        public OrchFunction(MainWindow parent)
        {
            this._parent = parent;
        }
        public int MaxParsCount => 2;

        public CallType CallType => CallType.Prefix;

        public string Symbol => "och";

        public int Precidence => 0;


        public  object Evaluate(IFsDataProvider parent, IParameterList pars)
        {
            var query = pars[0] as string;
            if (query == null)
                return new InvalidOperationException($"{this.Symbol} - {ParName(0)} is required");

            object parsVal = null;
            if (pars.Count > 1)
                parsVal = pars[1];

            var request = $"/api/query?access_token={Program.AccessToken}&query={System.Web.HttpUtility.UrlEncode(query)}";
            if (parsVal != null)
            {
                var json = new StringBuilder();
                FuncScript.Format(json, parsVal, null, false, true);
                request = $"{request}&pars={System.Web.HttpUtility.UrlEncode(json.ToString())}";
            }

            try
            {
                var ret = API.ApiGetAsync<object>(request);
                Console.Write($"Send orch query\n{query}\nWaiting...");
                ret.Wait();
                var res =FuncScript.NormalizeDataType(ret.Result);
                Console.WriteLine("Done");
                return res == null ? "null" : res;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                throw new Exception("Error running query on orck", ex);
            }
        }

        public string ParName(int index)
        {
            switch (index)
            {
                case 0:
                    return "Query";
                case 1:
                    return "Pars";
                default:
                    return null;
            }
        }
    }
}
