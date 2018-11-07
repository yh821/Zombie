#if AIMind
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class SequenceNode : Sequence
    {
#if AIMind
        public SequenceNode() : base("顺序节点", "由上往下执行，遇到false就返回") { }
#else
        public SequenceNode() { }
#endif
    }
}
