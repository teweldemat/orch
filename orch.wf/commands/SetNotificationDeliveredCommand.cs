using orch.core;

namespace orch.wf.commands
{
    [CommandType(
     TYPE_ID,
     COMMAND_TYPE_KEY,
     "Set Workflow Notification Delivered",
     initializer: typeof(SetNotificateDeliveredCommandInitializer),
     handler: typeof(SetNotificateDeliveredCommandHandler))]
    internal class SetNotificationDeliveredCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_WF_NOTIFICATION_DELIVERED";
        public const string TYPE_ID = "0e7472a5-e982-46c0-a719-b793d25524fc";

        public Guid NotificationId { get; set; }

        [OGeneratedData]
        public Guid UserId { get; set; }

        internal class SetNotificateDeliveredCommandInitializer : CommandInitializerBase<SetNotificationDeliveredCommand>
        {
            public SetNotificateDeliveredCommandInitializer(IOHost host) : base(host)
            {
            }

            public override void Init()
            {
                if (_commandData.NotificationId == Guid.Empty)
                {
                    throw new InvalidDataException("Please provide a valid Notification Id");
                }

                if (_commandData.UserId == Guid.Empty
                    && _commandInfo.UserId.HasValue && _commandInfo.UserId.Value != Guid.Empty)
                {
                    _commandData.UserId = _commandInfo.UserId.Value;
                }

                if (_commandData.UserId == Guid.Empty)
                {
                    throw new InvalidDataException("Please provide a valid User Id");
                }

            }
        }

        internal class SetNotificateDeliveredCommandHandler : CommandHandlerBase<SetNotificationDeliveredCommand>
        {
            private WfServiceCollection Services => (WfServiceCollection)_services;
            public SetNotificateDeliveredCommandHandler(WfServiceCollection services) : base(services)
            {
            }

            public override string Summarize(out bool html)
            {
                html = false;
                return $"Delivered Notification '{_commandData.NotificationId}' to User '{_commandData.UserId}'";
            }

            protected override void Execute()
            {
                Services.WfDb.SetNotificateDelivered(_commandInfo, _commandData.NotificationId, _commandData.UserId);
            }
        }
    }
}
