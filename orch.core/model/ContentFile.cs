using orch.common;

namespace orch.core.model
{
    public class ContentFile : ContentFileProps
    {
        public ContentFile() { }
        public ContentFile(ContentFileProps props)
            => this.MapFromBase(props);
    }
}
