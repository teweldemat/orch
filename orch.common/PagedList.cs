namespace orch.common
{
    public class PagedList<T>
    {
        public IList<T> List { get; set; } = new List<T>();
        public int Count { get; set; }
    }
}
