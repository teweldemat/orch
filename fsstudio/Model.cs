using funcscript;
using funcscript.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fsstudio
{
    internal enum VariableType
    {
        Standard,
        ClearText,
        TextTemplate
    }
    internal class VariableItem
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public bool Expanded { get; set; } = true;
        public VariableType Type { get; set; }= VariableType.Standard;
        
        public List<VariableItem> ChildItems=new List<VariableItem>();
        public object Evaluate(IFsDataProvider provider)
        {
            return this.Type switch
            {
                VariableType.Standard => FuncScript.Evaluate(provider, this.Expression),
                VariableType.ClearText => Expression,
                VariableType.TextTemplate => FuncScript.Evaluate(this.Expression, provider,null,FuncScript.ParseMode.FsTemplate),
                _=> throw new InvalidOperationException("Unknown variable type")
            }; ;
        }
    }
    internal class ExpressionSystem
    {
        public String SelectedItem = null;
        public List<VariableItem> Variables=new List<VariableItem>();
    }
}
