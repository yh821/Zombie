//#define AIMind
#if AIMind
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class NotNode : Decorator
    {
#if AIMind
        public NotNode() : base("非", "返回其子节点的相反结果") { }
#else
        public NotNode() { }

        public override bool Proc(IAIProc theOwner)
        {
            return !child.Proc(theOwner);
        }
#endif
    }
}
