namespace orch.console.commands
{
    [Command("clear")]
    public class ClearScreenCommand : SimpleCommand<ConsoleCommandHost>, ICommand
    {
        public ClearScreenCommand(ConsoleCommandHost host) : base(host)
        {
        }

        public override void Execute(string parameters)
        {
            Console.Clear();
        }
    }
}
