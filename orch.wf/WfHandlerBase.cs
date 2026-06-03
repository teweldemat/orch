using orch.core;
using orch.core.errors;
using orch.core.model;
using orch.wf.errors;
using orch.wf.model;

namespace orch.wf
{
    public abstract class WfHandlerBase : IWfHandler
    {
        protected WfServiceCollection _services;
        protected ActionConfigurationFormulaBase? _config;

        public WfHandlerBase(WfServiceCollection services)
        {
            _services = services;
            _config = LoadConfiguration();
        }
        public abstract ActionConfigurationFormulaBase LoadConfiguration();
        private void AssertConfiguration()
        {
            if (_config == null)
                throw new ConfigurationNotLoadedException(GetType());
        }
        public virtual RuleCheckResult IsActionTypeAvialable(Guid? thisTaskId, OTask? task, WfStateData? wfState, Guid actionTypeId)
        {
            try
            {
                AssertConfiguration();
                return _config!.CheckAction(new WfProvider(_services, null, task, wfState, null), actionTypeId, thisTaskId != wfState?.TaskId);
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationError($"Error evaluating CheckAction for {this.GetType()}, actionTypeId:{OTransactionService.GetTypeInfoById(actionTypeId)?.Type?.FullName ?? actionTypeId.ToString()}", ex);
            }
        }
        public virtual RuleCheckResult IsActionTypeAvialableForUser(Guid? thisTaskId, OTask? task, WfStateData? wfState, UserInfo user, Guid actionTypeId)
        {
            try
            {
                AssertConfiguration();
                return _config!.CheckUser(new WfProvider(_services, user, task, wfState, null), actionTypeId, thisTaskId != wfState?.TaskId);
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationError($"Error evaluating CheckUser for {this.GetType()}, actionTypeId:{OTransactionService.GetTypeInfoById(actionTypeId)?.Type?.FullName ?? actionTypeId.ToString()}", ex);
            }
        }
        public virtual RuleCheckResult IsActionAvialableForUser(Guid? thisTaskId, OTask? task, WfStateData? wfState, UserInfo user, Guid actionTypeId, object? action)
        {
            try
            {
                AssertConfiguration();
                return _config!.CheckUserAction(new WfProvider(_services, user, task, wfState, action), actionTypeId, thisTaskId != wfState?.TaskId);
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationError($"Error evaluating CheckUserAction for {this.GetType()}, actionTypeId:{OTransactionService.GetTypeInfoById(actionTypeId)?.Type?.FullName ?? actionTypeId.ToString()}", ex);
            }
        }
        public virtual void OnBeforeWaitedWfChanged(OCommand command, Guid monitorTask, WfStateData? monitoredTask, Guid actionTypeId, object? action)
        {
        }

        public virtual void OnAfterWaitedWfChanged(OCommand command, Guid monitorTask, WfStateData? monitoredTask, Guid actionTypeId, object? action)
        {
        }
    }
}
