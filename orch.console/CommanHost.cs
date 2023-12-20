namespace orch.console
{
    public class CommandAttribute : Attribute
    {
        private readonly string symbol;
        private readonly string name;

        public string Symbol => symbol;

        public string Name => name;

        public CommandAttribute(String symbol, String name)
        {
            this.symbol = symbol;
            this.name = name;
        }

        public CommandAttribute(String symbol) : this(symbol, symbol)
        {
        }
    }

    public interface ICommand
    {
        bool AcceptsStream { get; }

        bool InputStreamEnd();

        void ConsumeDataPacket(object data);

        bool HasOutput { get; }

        void Execute(String parameters);
    }

    public interface ICommandHost
    {
        TextWriter StdOut { get; }
        TextReader StdIn { get; }

        void CloseSession();
    }

    public class ConsoleCommandHost : ICommandHost
    {
        private readonly Action closeHandler;

        public TextWriter StdOut => Console.Out;

        public TextReader StdIn => Console.In;

        public void SetColor(ConsoleColor color) => Console.ForegroundColor = color;

        public void ResetColor() => Console.ResetColor();

        public ConsoleCommandHost(Action closeHandler = null)
        {
            this.closeHandler = closeHandler;
        }

        public void CloseSession()
        {
            closeHandler?.Invoke();
        }
    }

    public abstract class SimpleCommand<TCommandHost> : ICommand
            where TCommandHost : ICommandHost
    {
        public class FormFieldParseResult
        {
            public String error = null;
            public object Data;

            public FormFieldParseResult()
            { }

            public FormFieldParseResult(Object data)
            {
                this.Data = data;
                this.error = null;
            }

            public static FormFieldParseResult Error(String message)
            {
                return new FormFieldParseResult() { error = message, Data = null };
            }
        }

        public class FormField
        {
            public String Prompt;
            public Func<String, FormFieldParseResult> ParseFunc;
        }

        public static List<object> ReadForm(ICommandHost host, params FormField[] fields)
        {
            var ret = new List<object>();
            foreach (var field in fields)
            {
                do
                {
                    host.StdOut.Write($"{field.Prompt}: ");
                    var res = field.ParseFunc(host.StdIn.ReadLine());
                    if (res.error == null)
                    {
                        ret.Add(res.Data);
                        break;
                    }
                    else
                        host.StdOut.WriteLine(res.error);
                }
                while (true);
            }
            return ret;
        }

        protected readonly TCommandHost _host;

        public SimpleCommand(TCommandHost host)
        {
            this._host = host;
        }

        public virtual bool AcceptsStream => false;

        public virtual bool HasOutput => false;

        public virtual bool InputStreamEnd()
        {
            throw new NotImplementedException();
        }

        public virtual void ConsumeDataPacket(object data)
        {
            throw new NotImplementedException();
        }

        public abstract void Execute(string parameters);
    }
}