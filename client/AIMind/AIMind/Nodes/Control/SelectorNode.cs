#if AIMind
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class SelectorNode : Selector
    {
#if AIMind
        public SelectorNode() : base("选择节点", "由上往下执行，遇到true就返回") { }
#else
        public SelectorNode() { }
#endif
    }
}
