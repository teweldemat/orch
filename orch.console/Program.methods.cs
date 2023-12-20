using orch.console;

internal partial class Program
{
    public const String ACCESS_TOKEN_FILE_NAME = "access-token.txt";
    public const int MAX_HEARTBEAT_TRIES = 60;

    internal static void RunCommandLine(string[] args)
    {
        var commandSymbols = new Dictionary<string, Type>();
        foreach (var t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
        {
            var a = t.GetCustomAttributes(typeof(CommandAttribute), false);
            if (a != null && a.Length == 1)
            {
                commandSymbols.Add(((CommandAttribute)a[0]).Symbol, t);
            }
        }
        bool exitCalled = false;

        var commandHost = new ConsoleCommandHost(() => { exitCalled = true; });
        while (true)
        {
            String command;
            String parameters;
            String symbol;
            if (args.Length == 0)
            {
                Console.Write($"> ");
                command = Console.ReadLine();
                var sep = command.IndexOf(" ");

                if (sep == -1)
                {
                    symbol = command;
                    parameters = "";
                }
                else
                {
                    symbol = command.Substring(0, sep);
                    parameters = command.Substring(sep + 1);
                }
            }
            else
            {
                symbol = args[0];
                parameters = args.Length == 0 ? "" : args[1];
                for (int i = 2; i < args.Length; i++)
                    parameters += " " + args[i];
            }
            if (commandSymbols.TryGetValue(symbol, out var t))
            {
                var obj = Activator.CreateInstance(t, new object[] { commandHost }) as ICommand;
                try
                {
                    obj.Execute(parameters);
                    if (exitCalled)
                        return;
                }
                catch (Exception ex)
                {
                    Utility.DumpException(ex);
                }
            }
            else
                Console.WriteLine("Invalid command");
            if (args.Length > 0)
                break;
        }
    }
}