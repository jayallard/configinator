using System.Diagnostics;

namespace Allard.Configinator.Core.DocumentMerger
{
    [DebuggerDisplay("Layer={LayerName}, Transition={Transition}, Value={Value}")]
    public class PropertyLayer
    {
        public int LayerIndex { get; set; }
        public string LayerName { get; set; }
        public Transition Transition { get; set; }
        public object Value { get; set; }
    }
}