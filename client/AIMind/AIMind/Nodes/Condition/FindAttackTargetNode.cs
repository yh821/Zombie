//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class FindAttackTargetNode : ConditionConnectors
    {
#if AIMind
        public FindAttackTargetNode() : base("是否发现攻击目标", "") { }
#else
        public FindAttackTargetNode() { }

        public override bool Condition(IAIProc theOwner)
        {
            return theOwner.FindAttackTarget();
        }
#endif
    }
}
