//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class AttackTheTargetNode : Action
    {
#if AIMind
        public AttackTheTargetNode() : base("攻击目标", "") { }
#else
        public AttackTheTargetNode() { }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.AttackTheTarget();
        }
#endif
    }
}
