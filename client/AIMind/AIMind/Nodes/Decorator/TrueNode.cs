//#define AIMind
#if AIMind
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class TrueNode : Decorator
    {
#if AIMind
        public TrueNode() : base("真", "返回true结果") { }
#else
        public TrueNode() { }

        public override bool Proc(IAIProc theOwner)
        {
            child.Proc(theOwner);
            return true;
        }
#endif
    }
}
