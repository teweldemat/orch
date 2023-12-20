using orch.common;

namespace orch.wf.model
{
    public class OTaskType : OTaskTypeProps
    {
        public OTaskType() { }
        public OTaskType(OTaskTypeProps props)
        => this.MapFromBase(props);
    }
    public class WfNotification : WfNotificationProps
    {
        public WfNotification() { }
        public WfNotification(WfNotificationProps props)
        => this.MapFromBase(props);
    }

    public class WfNotificationTarget : WfNotificationTargetProps
    {
        public WfNotificationTarget() { }
        public WfNotificationTarget(WfNotificationTargetProps props)
        => this.MapFromBase(props);
    }
}
