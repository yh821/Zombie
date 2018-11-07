//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class StopMoveToNode : Action
    {
#if AIMind
        public StopMoveToNode() : base("停止移动", "") { }
#else
        public StopMoveToNode() { }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.StopMoveTo();
        }
#endif
    }
}
