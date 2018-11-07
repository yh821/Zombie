//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class CheckSelfNode : ConditionConnectors
    {
#if AIMind       
        public CheckSelfNode() : base("自检", "检查是否可以进入行为树") { }
#else
        public CheckSelfNode() { }

        public override bool Condition(IAIProc theOwner)
        {
            return theOwner.CheckSelf();
        }
#endif
    }
}