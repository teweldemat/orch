using funcscript;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using orch.core;
using System.Configuration;
using System.Reflection.Metadata;

namespace orch.console.commands
{
    [Command("update-command-table")]
    public class UnpdateCommandTable : SimpleCommand<ConsoleCommandHost>, ICommand
    {
        public UnpdateCommandTable(ConsoleCommandHost host) : base(host)
        {
        }

        public override void Execute(string parameters)
        {
            
            ApplicationScopeConfig appScopeConfig = new ApplicationScopeConfig();
            Program.Configuration.GetSection("ApplicationScopeConfig").Bind(appScopeConfig);
            using var service = IApplicationScopeFactory.LoadFromAssembly(appScopeConfig.AssemblyName, 
                appScopeConfig.TypeName);
            var t = service.Services.GetService<IOHost>().CurrentTime();
            service.Services.GetService<ITransactionDatabase>().UpdateCommandList(t);
        }
    }
}
