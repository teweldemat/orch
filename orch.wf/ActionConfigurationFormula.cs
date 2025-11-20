using FuncScript;
using FuncScript.Core;
using FuncScript.Model;
using orch.core;
using orch.wf.model;

namespace orch.wf
{
    public class WfActionFormula
    {
        public string ActionCheckFormula;
        public string UserCheckFormula;
        public string UserActionCheckFormula;
    }
    public abstract class ActionConfigurationFormulaBase
    {
        public abstract WfActionFormula Formula(Guid actionId);
        public RuleCheckResult CheckAction(KeyValueCollection provider, Guid actionId, bool returnTrueIfNotSet)
        {
            var f = AssertActionConfigSet(actionId);

            if (f == null || f.ActionCheckFormula == null)
            {
                if (returnTrueIfNotSet)
                    return new RuleCheckResult(true);
                throw new InvalidOperationException($"Check action formula not set for {OTransactionService.GetTypeInfoById(actionId).TypeName} in {this.GetType()}. Formula:{f?.ActionCheckFormula}");
            }
            return EvaluateFormula(provider, "Check Action", f.ActionCheckFormula);
        }
        public RuleCheckResult CheckUser(KeyValueCollection provider, Guid actionId, bool returnTrueIfNotSet)
        {
            var f = AssertActionConfigSet(actionId);
            if (f == null || f.ActionCheckFormula == null)
            {
                if (returnTrueIfNotSet)
                    return new RuleCheckResult(true);
                throw new InvalidOperationException($"Check user formula not set for {OTransactionService.GetTypeInfoById(actionId).TypeName}. Formula:{f?.ActionCheckFormula}");
            }

            return EvaluateFormula(provider, "Check User", f.UserCheckFormula);
        }
        public RuleCheckResult CheckUserAction(KeyValueCollection provider, Guid actionId, bool returnTrueIfNotSet)
        {
            var f = AssertActionConfigSet(actionId);
            if (f == null || f.UserActionCheckFormula == null)
            {
                if (returnTrueIfNotSet)
                    return new RuleCheckResult(true);
                throw new InvalidOperationException($"Check user action formula not set for {OTransactionService.GetTypeInfoById(actionId).TypeName}. Formula:{f?.ActionCheckFormula}");
            }
            return EvaluateFormula(provider, "Check User action", f.UserActionCheckFormula);
        }
        private WfActionFormula AssertActionConfigSet(Guid actionId)
        {
            var f = this.Formula(actionId);
            return f;
        }

        private RuleCheckResult EvaluateFormula(KeyValueCollection provider, string configName, string f)
        {
            object res;
            try
            {
                res = Engine.Evaluate(provider, f);
            }
            catch (FuncScript.Error.EvaluationException evex)
            {
                throw new InvalidOperationException($"Evaluation of {configName} failed", evex);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Evaluation of {configName} failed", ex);
            }
            if (res is bool yes)
                return new RuleCheckResult { Yes = yes, Reason = null };

            if (res is string reason)
                return new RuleCheckResult { Yes = false, Reason = reason };

            throw new InvalidOperationException($"{configName} evaluation returned invalid result: {(res == null ? "<null>" : res.ToString())}. Boolean value expected");
        }

        private TaskChange EvaluateTaskFormula(KeyValueCollection provider, string f)
        {
            if (f == null)
                return null;
            //parse the formula
            object res;
            try
            {
                res = Engine.Evaluate(provider, f);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Evaluation of task formula failed", ex);
            }
            if (!(res is FuncScript.Model.KeyValueCollection))
                throw new InvalidOperationException($"Evaluation of task formal returned invalid result: {(res == null ? "<null>" : res.ToString())}. Boolean value expected");
            return ((FuncScript.Model.KeyValueCollection)res).ConvertTo<TaskChange>();

        }

    }
    public class ActionConfigurationFormula : ActionConfigurationFormulaBase
    {
        public class ActionFormulaEntry
        {
            public Guid ActionTypeId;
            public WfActionFormula Formula;
            public ActionFormulaEntry(Guid key, WfActionFormula value)
            {
                this.ActionTypeId = key;
                this.Formula = value;
            }
        }
        public List<ActionFormulaEntry> ActionFormulas { get; set; }
        public override WfActionFormula Formula(Guid actionId)
        {
            var ret = this.ActionFormulas.Where(x => x.ActionTypeId == actionId).FirstOrDefault();
            if (ret == null)
                return null;
            return ret.Formula;
        }

    }
}
