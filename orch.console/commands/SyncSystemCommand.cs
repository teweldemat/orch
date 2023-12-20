using funcscript;
using orch.common;
using orch.core;
using orch.core.model;

namespace orch.console.commands
{
    [Command("sync")]
    class SyncSystemCommand : SimpleCommand<ConsoleCommandHost>
    {
        private readonly TimeSpan _pollingInterval;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private sealed class ProcessOptions
        {
            public bool Verbose = false;
        }

        public SyncSystemCommand(ConsoleCommandHost host) : base(host)
        {
            _pollingInterval = TimeSpan.FromSeconds(30);
        }

        public override void Execute(string parameters)
        {
            if (FuncScript.EvaluateSpaceSeparatedList(parameters) is not List<string> pars || pars.Count == 0)
            {
                _host.StdOut.WriteLine("Invalid command");
                return;
            }

            Task.Run(() => Run(pars));
        }

        private void Run(IList<string> parameters)
        {
            var parIndex = 0;
            var baseUri = parameters[parIndex++].ToString();
            var assemblyFile = parameters[parIndex++].ToString();
            var typeName = parameters[parIndex++].ToString();

            ProcessOptions options = new();
            for (int i = parIndex; i < parameters.Count; i++)
            {
                switch (parameters[i])
                {
                    case "-v":
                    case "--verbose":
                        options.Verbose = true;
                        break;
                }
            }

            var defaultErrorPollingInterval = TimeSpan.FromSeconds(5);
            var errorPollingInterval = defaultErrorPollingInterval;
            var maxErrorPollingInterval = TimeSpan.FromMinutes(2);

            try
            {
                string query = $@"core.GetTransactions(pars.seqNo, pars.count){{ 
                                    Count,
                                    List: List map (t) => t + {{ 
                                        Commands: core.GetCommandsOf(t.Id) 
                                    }}
                                }}";

                long seqNo = 0; // TODO: Get latest seqNo from Db
                short count = 100;

                using var client = new OrchApiClient(baseUri);
                using var service = IApplicationScopeFactory.LoadFromAssembly(assemblyFile, typeName);

                var accessToken = Guid.NewGuid(); // TODO: Access Token

                while (true)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    try
                    {
                        var result = client.ExecuteQueryAsync<PagedList<OTransaction>>(
                            accessToken: accessToken,
                            query: query,
                            pars: $"{{seqNo: {seqNo}, count: {count}}}"
                            )
                            .GetAwaiter()
                            .GetResult();

                        if (result.List.Count == 0)
                        {
                            Task.Delay(_pollingInterval, _cancellationTokenSource.Token).Wait();
                            continue;
                        }

                        foreach (var transaction in result.List)
                        {
                            foreach (var command in transaction.Commands)
                            {
                                var type = OTransactionService.GetTypeInfoById(command.DataTypeID);

                                if (type == null)
                                {
                                    throw new InvalidOperationException($"Invalid command type id {command.DataTypeID}");
                                }

                                var data = Newtonsoft.Json.JsonConvert.DeserializeObject(command.TextData, type.Type);

                                if (options.Verbose)
                                {
                                    _host.StdOut.WriteLine($"Executing\ncommand type:{type.Type}"
                                        + $"\nuserId: {command.UserId}"
                                        + $"\bsystemId: {command.SystemID}"
                                        + $"\ntypeId: {command.DataTypeID}"
                                        + $"\nmdData: \n{command.TextData}"
                                        );
                                }

                                service.ExecuteCommandUntyped(
                                    command.UserId,
                                    command.SystemID,
                                    command.DataTypeID,
                                    command.FormatVersion,
                                    data,
                                    out Guid tranId);
                            }
                            seqNo = transaction.SeqNo;
                        }

                        // Reset error polling interval upon successful execution
                        errorPollingInterval = defaultErrorPollingInterval;
                    }
                    catch (Exception ex)
                    {
                        _host.SetColor(ConsoleColor.DarkRed);

                        while (ex != null)
                        {
                            _host.StdOut.WriteLine($"{ex.Message}\n>{ex.StackTrace}");
                            ex = ex.InnerException;
                        }

                        _host.SetColor(ConsoleColor.DarkYellow);
                        _host.StdOut.WriteLine($"Retrying in {errorPollingInterval.TotalSeconds} seconds...");


                        Task.Delay(errorPollingInterval, _cancellationTokenSource.Token).Wait();

                        // Exponential backoff for error handling
                        errorPollingInterval = TimeSpan.FromTicks(errorPollingInterval.Ticks * 2);

                        // Cap the polling interval at a maximum value
                        if (errorPollingInterval > maxErrorPollingInterval)
                        {
                            errorPollingInterval = maxErrorPollingInterval;
                        }
                    }
                    finally
                    {
                        _host.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                _host.SetColor(ConsoleColor.DarkRed);
                _host.StdOut.WriteLine($"An unexpected error occurred: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _host.SetColor(ConsoleColor.Green);
                _host.StdOut.WriteLine("Sync process has ended.");
                _host.ResetColor();

            }
        }

        public void Cancel()
        {
            _host.StdOut.WriteLine("Cancelling sync process...");
            _cancellationTokenSource.Cancel();
        }
    }
}
