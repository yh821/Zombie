//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class CheckAttackRangeNode : ConditionConnectors
    {
#if AIMind
        public CheckAttackRangeNode() : base("是否在攻击范围内", "") { }
#else
        public CheckAttackRangeNode() { }

        public override bool Condition(IAIProc theOwner)
        {
            return theOwner.CheckAttackRange();
        }
#endif
    }
}
