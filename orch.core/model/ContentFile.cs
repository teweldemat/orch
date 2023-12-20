using orch.common;
using ProtoBuf;

namespace orch.core.model
{
    [ProtoContract]
    public class ContentFile : ContentFileProps
    {
        public ContentFile() { }
        public ContentFile(ContentFileProps props)
            => this.MapFromBase(props);
    }

}
