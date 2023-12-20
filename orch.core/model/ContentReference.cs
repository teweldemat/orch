using orch.common;

namespace orch.core.model
{
    public class ContentReference : ContentReferenceProps
    {
        public ContentReference() { }
        public ContentReference(ContentReferenceProps props)
            => this.MapFromBase(props);
    }

}
