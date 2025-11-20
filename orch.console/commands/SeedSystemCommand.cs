using System;
using FuncScript;
using FuncScript.Model;
using FuncScript.Functions;
using Microsoft.Extensions.DependencyInjection;
using orch.core;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using FuncScript.Sql.Core;

namespace orch.console.commands
{
    [Command("seed")]
    public class SeedSystemCommand : SimpleCommand<ConsoleCommandHost>
    {
        public SeedSystemCommand(ConsoleCommandHost host) : base(host)
        {
        }

        private static bool ExecuteInDirectory(string directory, Func<bool> action)
        {
            var cur = Directory.GetCurrentDirectory();
            var targetDirectory = Path.GetFullPath(directory);
            Directory.SetCurrentDirectory(targetDirectory);
            try
            {
                return action();
            }
            finally
            {
                Directory.SetCurrentDirectory(cur);
            }
        }

        private sealed class ProcessOptions
        {
            public bool Isolated = false;
            public bool Verbose = false;
            public bool IgnoreCount = false;
            public int SkipCommands = 0;
            public bool Atomic = false;
        }

        private OTransactionService currentService;
        private Dictionary<string, Guid> Attachments;

        public Guid? GetAttachment(string fileName)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                return null;
            if (Attachments.ContainsKey(fi.FullName.ToLower()))
                return Attachments[fi.FullName.ToLower()];
            var c = currentService.Services.GetService<ISystemDatabase>().SaveFile(fi.Name, fi.OpenRead());
            Attachments.Add(fi.FullName.ToLower(), c.FileId);
            return c.FileId;
        }

        private bool ProcessFile(
            OTransactionService service,
            string fileName,
            ProcessOptions options,
            KeyValueCollection vars,
            out KeyValueCollection outvars,
            int nPrevCommands,
            out int nCommands)
        {
            outvars = null;
            nCommands = nPrevCommands;

            var tranDb = service.Services.GetRequiredService<ITransactionDatabase>();

            try
            {
                var rootUser = tranDb.GetRootUser();

                var p = new KvcProvider(new ObjectKvc(new
                {
                    attach = new Func<string, Guid?>(GetAttachment),
                    sql = new SqlFunction(),
                }), new ViewQueryProvider(service, rootUser == null ? Guid.Empty : rootUser.Id, null));

                var mainFile = fileName + ".fx";
                var fi = new FileInfo(mainFile);

                if (!File.Exists(mainFile))
                {
                    _host.StdOut.WriteLine($"Couldn't find file {mainFile}");
                    return false;
                }

                var mainFileContent = File.ReadAllText(mainFile);
                KeyValueCollection seedData = null;
                KeyValueCollection thisVar = null;
                object fromValue = null;

                var res = ExecuteInDirectory(fi.DirectoryName, () =>
                {
                    _host.StdOut.WriteLine($"Evaluating {mainFile}");
                    seedData = Engine.Evaluate(p, mainFileContent) as KeyValueCollection;
                    if (seedData == null)
                    {
                        _host.StdOut.WriteLine($"Seed file didn't evaluate to kvc");
                        return false;
                    }

                    var evaluatedVars = seedData.Get("vars") as KeyValueCollection;
                    thisVar = KeyValueCollection.Merge(vars, evaluatedVars);

                    if (!options.Isolated)
                    {
                        fromValue = seedData.Get("from");
                    }
                    return true;
                });

                if (!res)
                    return false;

                if (!options.Isolated)
                {
                    IList<string> baseFiles = null;

                    if (fromValue is string @string)
                        baseFiles = new string[] { @string };

                    if (fromValue is FsList list)
                    {
                        baseFiles = list
                            .Select(x => x as string)
                            .Where(x => x != null)
                            .ToList();
                    }
                    if (baseFiles != null && baseFiles.Count > 0)
                    {
                        int nc = nCommands;
                        foreach (var baseFile in baseFiles)
                        {
                            var execRes = ExecuteInDirectory(fi.DirectoryName, () =>
                            {
                                var ret = ProcessFile(service, Path.Combine(fi.Directory.FullName, baseFile), options, thisVar, out thisVar, nc, out nc);
                                return ret;
                            });

                            if (!execRes)
                            {
                                return false;
                            }
                        }
                        nCommands = nc;
                    }
                }

                var commandFile = fileName + ".commands.fx";

                var commandProvider = new KvcProvider(new ObjectKvc(
                    new
                    {
                        vars = thisVar,
                        sql = new SqlFunction(),
                    }), p);

                if (!File.Exists(commandFile))
                {
                    _host.StdOut.WriteLine($"No command file {commandFile}. Skipping");
                    outvars = thisVar;
                    return true;
                }

                _host.StdOut.WriteLine($"Processing {fi.FullName}");

                var commandFileContent = File.ReadAllText(commandFile);
                FsList commands = null;
                res = ExecuteInDirectory(fi.DirectoryName, () =>
                {
                    _host.StdOut.WriteLine($"Evaluating {commandFile}");

                    commands = Engine.Evaluate(commandProvider, commandFileContent) as FsList;
                    if (commands == null)
                    {
                        _host.StdOut.WriteLine($"No command was found in {commandFile}");
                        return false;
                    }
                    return true;
                });
                if (!res)
                    return false;
                var index = nCommands;

                var commandExecutionResult = ExecuteInDirectory(fi.DirectoryName, () =>
                {
                    foreach (KeyValueCollection c in commands)
                    {
                        if (c == null)
                            continue;
                        index++;
                        var commandType = c.Get("cmd") as string;
                        var user = c.Get("user") as string;
                        var data = c.Get("data") as KeyValueCollection;
                        CommandTypeInfo cmdt;
                        if (commandType == null || (cmdt = OTransactionService.GetTypeIdByKey(commandType)) == null)
                        {
                            _host.StdOut.WriteLine($"Invalid command type {commandType}");
                            return false;
                        }
                        var userInfo = tranDb.GetUserInfo(user);
                        var systemInfo = tranDb.GetCurrentSystemInformation();

                        _host.StdOut.Write($"{index}: {commandType}...");
                        if ((options.Isolated || systemInfo == null || options.IgnoreCount || index > tranDb.Count) && index > options.SkipCommands)
                        {
                            Guid? userId = userInfo == null ? null : userInfo.Id;
                            Guid? systemId = systemInfo == null ? null : systemInfo.SystemId;
                            Guid typeId = cmdt.TypeId;
                            object cmdData = data.ConvertTo(cmdt.Type);
                            if (options.Verbose)
                            {
                                _host.StdOut.WriteLine($"Executing command type:{cmdt.Type}"
                                    + $"\nuserId: {userId}"
                                    + $"\bsystemId: {systemId}"
                                    + $"\ntypeId: {typeId}"
                                    + $"\nmdData: \n{(cmdData == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(cmdData))}"
                                    );
                            }


                            var commandId = service.ExecuteCommandUntyped(
                                userId,
                                systemId,
                                typeId,
                                0,
                                cmdData, out _);


                            var command = tranDb.GetCommand(commandId);

                            if (!string.IsNullOrWhiteSpace(command.TextSummary))
                            {
                                var plainText = Regex.Replace(command.TextSummary, "<.*?>", ""); // Strip HTML tags
                                plainText = HttpUtility.HtmlDecode(plainText); // Decode HTML entities
                                _host.StdOut.WriteLine($"OK - {plainText}");
                            }
                            else
                                _host.StdOut.WriteLine($"OK");
                        }
                        else
                            _host.StdOut.WriteLine("Skipped");
                    }

                    return true;
                });

                if (!commandExecutionResult)
                {
                    return false;
                }

                outvars = thisVar;
                nCommands = index;


                return true;
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    _host.StdOut.WriteLine(ex.Message);
                    _host.StdOut.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                }
                return false;
            }
            finally
            {
            }
        }

        public override void Execute(string parameters)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var failed = false;

            try
            {
                var parsedParameters = Engine.EvaluateSpaceSeparatedList(parameters);
                if (parsedParameters == null)
                {
                    _host.StdOut.WriteLine("Invalid command");
                    return;
                }


                if (parsedParameters is not IEnumerable<string> pars || pars.Count() == 0)
                {
                    _host.StdOut.WriteLine("Invalid command");
                    return;
                }


                var parsList = pars.ToArray();
                var parIndex = 0;
                var assemblyFile = parsList[parIndex++].ToString();
                var typeName = parsList[parIndex++].ToString();


                using var service = IApplicationScopeFactory.LoadFromAssembly(assemblyFile, typeName);

                var seedIndexFileName = parsList[parIndex++].ToString();
                var options = new ProcessOptions();
                for (int i = parIndex; i < parsList.Length; i++)
                    switch (parsList[i].ToString())
                    {
                        case "--isolated":
                            options.Isolated = true;
                            break;

                        case "-v":
                        case "--verbose":
                            options.Verbose = true;
                            break;

                        case "--ignore-count":
                            options.IgnoreCount = true;
                            break;

                        case "--skip":
                            if (i + 1 < parsList.Length && int.TryParse(parsList[i + 1], out int skip))
                            {
                                options.SkipCommands = skip;
                                i++; // Skip the next argument since we've processed it
                            }
                            break;

                        case "--atomic":
                            options.Atomic = true;
                            break;
                    }

                currentService = service;
                Attachments = new Dictionary<string, Guid>();

                var tranDb = service.Services.GetRequiredService<ITransactionDatabase>();

                if (options.Atomic)
                    tranDb.BeginTransaction();


                if (ProcessFile(service, seedIndexFileName, options, null, out var vars, 0, out var n))
                {
                    if (vars == null)
                        _host.StdOut.WriteLine($"No variables applied!");
                    else
                    {
                        var tempDir = Path.Combine(AppContext.BaseDirectory, "temp");
                        Directory.CreateDirectory(tempDir);

                        var fn = Path.Combine(tempDir, "vars.fx");
                        File.WriteAllText(fn, vars.ToString());
                        _host.StdOut.WriteLine($"Applied variables saved in {new Uri(fn).AbsoluteUri}");
                    }
                        
                    if (options.Atomic)
                        tranDb.CommitTransaction();
                } 
                else
                {
                    if (options.Atomic)
                        tranDb.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                failed = true;
                while (ex != null)
                {
                    _host.StdOut.WriteLine($"{ex.Message}\n>{ex.StackTrace}");
                    ex = ex.InnerException;
                }
            }
            finally
            {
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                if (failed)
                {
                    _host.SetColor(ConsoleColor.Red);
                }
                else
                {
                    _host.SetColor(ConsoleColor.Green);
                }
                _host.StdOut.WriteLine($"Total execution time: {elapsedTime.Hours} hours, {elapsedTime.Minutes} minutes, {elapsedTime.Seconds} seconds.");
                _host.ResetColor();
            }
        }
    }
}
