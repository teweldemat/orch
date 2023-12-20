using orch.common;
using orch.core.model;

namespace orch.core.ef.System.Entities
{
    public class DALContentFile : ContentFileProps
    {
        public DALContentFile()
        { }

        public DALContentFile(ContentFileProps props)
            => this.MapFromBase(props);
    }
}