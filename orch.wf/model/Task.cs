using orch.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.wf.model
{
    
    public class OTask : OTaskProps
    {
        public OTask() { }
        public OTask(OTaskProps props)
        => this.MapFromBase(props);
        public IList<CheckListItem> CheckList { get; set; }
    }
    
    
    public class CheckListStatus : CheckListStatusProps
    {
        public CheckListStatus() { }
        public CheckListStatus(CheckListStatusProps props)
        => this.MapFromBase(props);
    }
    
    public class CheckListItem : CheckListItemProps
    {
        public CheckListItem() { }
        public CheckListItem(CheckListItemProps props)
        => this.MapFromBase(props);
    }
    
}
