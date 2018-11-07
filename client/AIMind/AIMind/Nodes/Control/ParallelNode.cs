#if AIMind
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class ParallelNode : Parallel
    {
#if AIMind
        public ParallelNode() : base("并行节点", string.Empty) { }
#else
        public ParallelNode() { }
#endif
    }
}
