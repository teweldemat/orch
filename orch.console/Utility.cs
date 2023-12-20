using Newtonsoft.Json;
using orch.utils.web;
using System.Net.Http.Json;
using System.Text;

namespace orch.console
{
    internal class Utility
    {
        private static String urlBase;

        internal class MenuItem
        {
            public String Description { get; set; }
            public Action Action { get; set; }
            public String Selector { get; set; }
        }

        public static void Menu(String title, params MenuItem[] items)
        {
            try
            {
                Console.WriteLine(title);
                var index = 1;
                var list = new Dictionary<string, MenuItem>();
                foreach (var i in items)
                {
                    if (i.Selector == null)
                        i.Selector = index.ToString();
                    Console.WriteLine($"{i.Selector}: {i.Description}");
                    index++;
                    list.Add(i.Selector, i);
                }
                do
                {
                    Console.Write(":");
                    var s = Console.ReadLine();
                    if (list.ContainsKey(s))
                    {
                        Console.WriteLine(list[s].Description);
                        list[s].Action();
                        return;
                    }
                    else
                        Console.WriteLine("Invalid selection");
                }
                while (true);
            }
            catch (Exception ex)
            {
                Utility.DumpException(ex);
            }
        }

        internal static String ReadPassword()
        {
            var pw = new StringBuilder();
            do
            {
                var ch = Console.ReadKey(true);
                if (ch.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return pw.ToString();
                }
                pw.Append(ch.KeyChar);
            } while (true);
        }

        private static void WriteLineWithColor(String line, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void DumpException(Exception ex)
        {
            if (ex is AggregateException)
            {
                foreach (var inner in ((AggregateException)ex).InnerExceptions)
                    DumpException(inner);
            }
            else
            {
                var prefix = "";
                while (ex != null)
                {
                    WriteLineWithColor($"{prefix}{ex.ToString()}", ConsoleColor.Red);
                    WriteLineWithColor($"{ex.StackTrace}", ConsoleColor.Cyan);
                    ex = ex.InnerException;
                    prefix = ">" + prefix;
                }
            }
        }

        static Utility()
        {
            urlBase = System.Configuration.ConfigurationManager.AppSettings["api-url"];
        }

        public static async Task<T> ApiGetAsync<T>(string url)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(urlBase);
            var res = await client.GetAsync(url);
            await HandleError(res);
            return await res.Content.ReadFromJsonAsync<T>();
        }

        private static async Task HandleError(HttpResponseMessage res)
        {
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
                return;
            var error = await res.Content.ReadAsStringAsync();
            ErrorInfo errorInfo;
            try
            {
                errorInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorInfo>(error);
            }
            catch
            {
                errorInfo = null;
            }

            if (errorInfo != null)
            {
                String msg = errorInfo.Error;
                if (errorInfo.Exceptions != null)
                {
                    var prefix = "";
                    foreach (var d in errorInfo.Exceptions)
                    {
                        if (msg == null)
                            msg = $"{d.Message}\n{d.StackTrace}";
                        else
                        {
                            prefix += ">";
                            msg += $"\n{prefix}{d.Message}\n{d.StackTrace}";
                        }
                    }
                }
                throw new Exception(msg);
            }
            throw new Exception("Unknown error");
        }

        internal static async Task ApiDeleteAsync(string url)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(urlBase);
            var res = await client.DeleteAsync(url);
            await HandleError(res);
        }

        public static async Task ApiPostNoReturn<PostType>(string url, PostType data)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(urlBase);
            var res = await client.PostAsJsonAsync(url, data);
        }

        public static async Task<ReturnType> ApiPostWithReturn<ReturnType, PostType>(string url, PostType data)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(urlBase);
            var res = await client.PostAsJsonAsync(url, data);
            await HandleError(res);
            var str = await res.Content.ReadAsStringAsync();
            var ret = JsonConvert.DeserializeObject<ReturnType>(str);
            return ret;
        }
    }
}