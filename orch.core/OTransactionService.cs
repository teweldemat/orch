using Newtonsoft.Json;
using orch.core.command;
using orch.core.errors;
using orch.core.model;

namespace orch.core
{
    /// <summary>
    /// The core service of the transaction framework
    /// This instance methods of this object are trade unsafe!
    /// </summary>
    public partial class OTransactionService : IDisposable
    {
        protected readonly IOHost _host;
        protected readonly IServiceProvider _services;

        public ITransactionDatabase Db { get; private set; }

        public OTransactionService(IServiceProvider services, IOHost host, ITransactionDatabase db)
        {
            _services = services;
            _host = host;

            Db = db;
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Db.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IServiceProvider Services => _services;

        protected Stack<OCommand> DataStack; //this is thread safe as we don't share the instance of the service between threads
        protected Stack<ICommandHandler> HandlersStack;

        protected OTransaction CurrentTran;
        protected int DataSeqNo = 1;

        protected virtual void ProcessCommand(
            OCommand command,
            object data,
            bool mainCommand,
            out ICommandHandler handler)
        {
            command.Id = _host.NextGuid();
            command.TranId = CurrentTran.Id;
            command.SeqNo = DataSeqNo;
            command.UserId = CurrentTran.UserId;
            command.SystemID = CurrentTran.SystemID;
            command.Time = CurrentTran.Time;
            command.MainCommand = mainCommand;

            var initializer = GetInitializer(command.DataTypeID);

            if (initializer != null)
            {
                initializer.SetData(CurrentTran, command, data);
                initializer.Init();
            }

            handler = GetHandler(command.DataTypeID);

            if (handler != null)
            {
                handler.SetData(CurrentTran, command, data, mainCommand);
                handler.Preprocess();
                command.TextSummary = handler.Summarize();
                HandlersStack.Push(handler);
            }

            command.TextData = JsonConvert.SerializeObject(data);
        }

        protected virtual Guid ExecuteChildTransactionUntyped(OCommand command, object data)
        {
            var top = DataStack.Peek();
            DataSeqNo++;
            command.ParentSeqNo = top.SeqNo;
            command.TranId = CurrentTran.Id;
            command.SystemID = CurrentTran.SystemID;

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

        private Guid ExecuteCommandUntyped(
            OTransaction tran,
            OCommand command,
            object data,
            out Guid tranId)
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

                tran.Id = _host.NextGuid();
                tran.Time = _host.CurrentTime();

                command.ParentSeqNo = null;

                var sysInfo = Db.GetCurrentSystemInformation();

                bool emptySystem = sysInfo == null;

                if (emptySystem)
                {
                    Bootstrap(tran, command, data, out sysInfo);
                }
                else
                {
                    if (command.DataTypeID == Guid.Parse(SetSystemIdCommand.TYPE_ID))
                        throw new InvalidOperationException("Set system ID not allowed");
                    
                    SystemIdMismatchException.ThrowIfNotEqual(sysInfo.SystemId, tran.SystemID);

                    tran.PrevId = sysInfo.HeadTranId;
                }
                ProcessCommand(command, data, true, out var h);
                tran.TextSummary = command.TextSummary;

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

                Db.UpdateSystemInformation(command, sysInfo, emptySystem); // Update SystemInfo

                if (!inTrans)
                    Db.CommitTransaction();

                // Call PostExecute only if the transaction is no longer active,
                // indicating it was committed successfully.
                if (!Db.InTransaction)
                {
                    while (HandlersStack.Count > 0)
                    {
                        var handler = HandlersStack.Pop();
                        try
                        {
                            handler.PostExecute();
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(
                                "Command executed successfully but an error occurred during post-execution.",
                                ex);
                        }
                    }
                }

                tranId = tran.Id;
                return command.Id;
            }
            catch
            {
                if (!inTrans)
                    Db.RollbackTransaction();

                throw;
            }
        }

        public virtual Guid ExecuteCommandUntyped(
            Guid? userId,
            Guid? systemId,
            Guid typeId,
            int formatVersion,
            object data,
            out Guid tranId)
        {
            var tran = new OTransaction
            {
                Id = _host.NextGuid(),
                UserId = userId,
                SystemID = systemId,
            };
            var command = new OCommand
            {
                Id = _host.NextGuid(),
                FormatVersion = formatVersion,
                DataTypeID = typeId,
                UserId = userId
            };

            return ExecuteCommandUntyped(tran, command, data, out tranId);
        }

        public virtual Guid ExecuteCommand<DataType>(
            OTransaction tran,
            OCommand command,
            DataType data,
            out Guid tranId)
        {
            var type = GetTypeIdByType(typeof(DataType))
                ?? throw new InvalidOperationException($"{typeof(DataType)} is not command data type");

            command.DataTypeID = type.TypeId;

            return ExecuteCommandUntyped(tran, command, data, out tranId);
        }

        public virtual Guid ExecuteCommand<DataType>(
            Guid? userId,
            Guid? systemId,
            int formatVersion,
            DataType data,
            out Guid tranId)
        {
            var tran = new OTransaction { UserId = userId, SystemID = systemId, };
            var command = new OCommand { FormatVersion = formatVersion, UserId = userId };
            return ExecuteCommand(tran, command, data, out tranId);
        }

        public virtual Guid ExecuteChildCommandTyped<DataType>(int formatVersion, DataType data)
        {
            var command = new OCommand { FormatVersion = formatVersion, };
            return ExecuteChildCommandTyped(command, data);
        }

        private Guid ExecuteChildCommandTyped<T>(OCommand command, T data)
        {
            var type = GetTypeIdByType(typeof(T))
                ?? throw new InvalidOperationException($"{typeof(T)} is not cmmand data type");

            command.DataTypeID = type.TypeId;
            return ExecuteChildTransactionUntyped(command, data);
        }

        public bool IsBootStrapped()
        {
            return Db.GetHeadTransaction() != null;
        }

        public bool IsRootUserDefined()
        {
            return Db.GetRootUser() != null;
        }

        public bool IsRootUser(Guid userId)
        {
            return (!userId.Equals(Guid.Empty)) && (Db.GetRootUser()?.Id == userId);
        }
        
        public bool IsSystemUser(Guid userId)
        {
            return (!userId.Equals(Guid.Empty)) && (Db.GetSystemUser()?.Id == userId);
        }

        private void Bootstrap(
            OTransaction tran,
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

            tran.PrevId = null;

            tran.TextSummary = "<p>System id set</p>";

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
    }
}
