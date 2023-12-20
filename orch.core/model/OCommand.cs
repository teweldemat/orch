using orch.common;

namespace orch.core.model
{
    public class OCommand:OCommandProps
    {
        /// <summary>
        /// The unique id of the transaction system. This id must match the system id set at the first ever transaction
        /// This data is not persisted as all valid transactions should have the same id as the system
        /// </summary>
        public Guid? SystemID { get; set; }
        

        public OCommand() { }
        public OCommand(OCommandProps props)
            => this.MapFromBase(props);
    }
    

}