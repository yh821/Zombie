//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class FindFollowTargetNode : ConditionConnectors
    {
#if AIMind
        public FindFollowTargetNode() : base("是否发现跟随目标", "") { }
#else
        public FindFollowTargetNode() { }

        public override bool Condition(IAIProc theOwner)
        {
            return theOwner.FindFollowTarget();
        }
#endif
    }
}
