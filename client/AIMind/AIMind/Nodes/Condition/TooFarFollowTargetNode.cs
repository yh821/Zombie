//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class TooFarFollowTargetNode : ConditionConnectors
    {
#if AIMind
        public TooFarFollowTargetNode() : base("是否离跟随目标太远", "") { }
#else
        public TooFarFollowTargetNode() { }

        public override bool Condition(IAIProc theOwner)
        {
            return theOwner.TooFarFollowTarget();
        }
#endif
    }
}
