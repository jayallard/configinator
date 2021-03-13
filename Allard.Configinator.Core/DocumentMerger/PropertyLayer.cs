namespace Allard.Configinator.Core.DocumentMerger
{
    public class PropertyLayer
    {
        public int LayerIndex { get; set; }
        public string LayerName { get; set; }
        public Transition Transition { get; set; }
        public object Value { get; set; }
    }
}