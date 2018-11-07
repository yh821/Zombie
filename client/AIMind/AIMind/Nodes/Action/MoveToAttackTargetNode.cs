//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class MoveToAttackTargetNode : Action
    {
#if AIMind
        public MoveToAttackTargetNode() : base("移动向攻击目标", "") { }
#else
        public MoveToAttackTargetNode() { }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.MoveToAttackTarget();
        }
#endif
    }
}
