using ProtoBuf;

namespace orch.common
{
    [ProtoContract]
    public class PagedList<T>
    {
        [ProtoMember(1)]
        public IList<T> List { get; set; } = new List<T>();

        [ProtoMember(2)]
        public int Count { get; set; }
    }
}
