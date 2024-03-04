using Newtonsoft.Json;
using orch.core.command;
using orch.core.model;

namespace orch.core
{
    public partial class OTransactionReplayService : OTransactionService
    {
        public OTransactionReplayService(
            IServiceProvider services,
            IOHost host,
            ITransactionDatabase db) : base(services, host, db) { }

        protected override void ProcessCommand(
            OCommand command,
            object data,
            bool mainCommand,
            out ICommandHandler handler
        )
        {
            handler = GetHandler(command.DataTypeID);

            if (handler != null)
            {
                handler.SetData(CurrentTran, command, data, mainCommand);
                handler.Preprocess();
                HandlersStack.Push(handler);
            }
        }


        private Stack<OCommand> ReplayStack;

        public void ReplayTransaction(OTransaction transaction, List<OCommand> commands)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            if (commands == null)
                throw new ArgumentNullException(nameof(commands));

            var orderedCommands = commands.OrderByDescending(c => c.SeqNo).ToList();

            var mainCommand =
                orderedCommands.FirstOrDefault(c => c.MainCommand) ?? throw new InvalidOperationException("No main command found in the command list.");

            ReplayStack = new Stack<OCommand>();

            foreach (var command in orderedCommands.Where(c => !c.MainCommand))
            {
                ReplayStack.Push(command);
            }

            var ttInfo =
                GetTypeInfoById(mainCommand.DataTypeID)
                ?? throw new InvalidOperationException(
                    $"Command type not found for command Id {mainCommand.Id}."
                );

            var data = JsonConvert.DeserializeObject(mainCommand.TextData, ttInfo.Type);

            ExecuteCommandUntyped(transaction, mainCommand, data, out _);
        }

        private Guid ExecuteChildTransactionUntyped(OCommand command)
        {
            if (!ReplayStack.Any())
            {
                var typeInfo = GetTypeInfoById(command.DataTypeID);
                throw new InvalidOperationException($"Replay Stack is empty. Expected Command Type: '{command.DataTypeID}' - '{typeInfo.Key}'");
            }

            if (ReplayStack.Peek().DataTypeID != command.DataTypeID)
            {
                var expectedCommand = ReplayStack.Peek();
                var expectedTypeInfo = GetTypeInfoById(expectedCommand.DataTypeID);
                var commandTypeInfo = GetTypeInfoById(command.DataTypeID);
                throw new InvalidOperationException($"Replay Stack is inconsistent. Expected '{command.DataTypeID}' - '{commandTypeInfo.Key}', Found: '{expectedCommand.DataTypeID}' - '{expectedTypeInfo.Key}'");
            }

            command = ReplayStack.Pop();
            var data = JsonConvert.DeserializeObject(
                command.TextData,
                GetTypeInfoById(command.DataTypeID).Type
            );

            ProcessCommand(command, data, false, out var h);

            Db.AddCommand(command);
            if (h != null)
            {
                DataStack.Push(command);
                try
                {
                    h.Execute();
                }
                finally
                {
                    DataStack.Pop();
                }
            }
            return command.Id;
        }

        private void ExecuteCommandUntyped(
            OTransaction tran,
            OCommand command,
            object data,
            out Guid tranId
        )
        {
            var inTrans = Db.InTransaction;

            if (!inTrans)
                Db.BeginTransaction();
            try
            {
                DataSeqNo = 1;
                CurrentTran = tran;
                DataStack = new Stack<OCommand>();
                HandlersStack = new Stack<ICommandHandler>();

                var sysInfo = Db.GetCurrentSystemInformation();

                bool emptySystem = sysInfo == null;

                if (emptySystem)
                {
                    Bootstrap(command, data, out sysInfo);
                }
                else
                {
                    if (command.DataTypeID == Guid.Parse(SetSystemIdCommand.TYPE_ID))
                        throw new InvalidOperationException("Set system ID not allowed");

                    if (sysInfo.SystemId != tran.SystemID)
                        throw new InvalidOperationException(
                            $"Transaction system id {command.SystemID} doesn't match the current system id {sysInfo.SystemId}"
                        );
                }

                ProcessCommand(command, data, true, out var h);

                Db.AddTransaction(tran);

                Db.AddCommand(command);

                if (h != null)
                {
                    DataStack.Push(command);
                    try
                    {
                        h.Execute();
                    }
                    finally
                    {
                        DataStack.Pop();
                    }
                }
                sysInfo.HeadTranId = tran.Id;
                sysInfo.LastTranTime = tran.Time;

                Db.UpdateSystemInformation(command, sysInfo, emptySystem);

                if (!inTrans)
                    Db.CommitTransaction();

                tranId = tran.Id;
            }
            catch
            {
                if (!inTrans)
                    Db.RollbackTransaction();

                throw;
            }
        }

        public override Guid ExecuteChildCommandTyped<DataType>(int formatVersion, DataType data)
        {
            var type =
                GetTypeIdByType(typeof(DataType))
                ?? throw new InvalidOperationException($"{typeof(DataType)} is not cmmand data type");

            // The actual command data will later be extracted from the replay stack.
            var placeholder = new OCommand() { DataTypeID = type.TypeId };

            return ExecuteChildTransactionUntyped(placeholder);
        }

        private void Bootstrap(
            OCommand command,
            object data,
            out TransactionSystemInformation sysInfo)
        {
            if (command.DataTypeID != Guid.Parse(SetSystemIdCommand.TYPE_ID))
            {
                throw new InvalidOperationException(
                    $"Only {SetSystemIdCommand.COMMAND_TYPE_KEY} transaction is allowed for an empty system"
                );
            }

            sysInfo = new TransactionSystemInformation
            {
                SystemId = ((SetSystemIdCommand)data).SystemId
            };

            Db.CreateUser(
                command,
                new UserInfo
                {
                    Id = _host.NextGuid(),
                    UserName = UserInfoProps.USER_NAME_SYSTEM,
                    FullName = "User for system initiated operations",
                    Time = _host.CurrentTime()
                }
            );
        }


        public override Guid ExecuteCommand<DataType>(
            OTransaction tran,
            OCommand command,
            DataType data,
            out Guid tranId)
        {
            throw new NotSupportedException(
                $"'{nameof(ExecuteCommand)}' is not supported during replay.");
        }

        public override Guid ExecuteCommand<DataType>(
            Guid? userId,
            Guid? systemId,
            int formatVersion,
            DataType data,
            out Guid tranId)
        {
            throw new NotSupportedException(
                $"'{nameof(ExecuteCommand)}' is not supported during replay.");
        }
    }
}
