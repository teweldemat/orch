namespace orch.common
{
    public class SpatialCluster<T>
    {
        public int? ClusterId { get; set; }
        public IList<T> List { get; set; } = new List<T>();
    }
}
