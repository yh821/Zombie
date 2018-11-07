//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class MoveToFollowTargetNode : Action
    {
#if AIMind
        public MoveToFollowTargetNode() : base("移动向跟随目标", "") { }
#else
        public MoveToFollowTargetNode() { }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.MoveToFollowTarget();
        }
#endif
    }
}
