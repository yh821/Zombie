//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class MoveByForwardInThinkCountNode : Action
    {
        protected int thinkCount = 0;

#if AIMind
        [DesignerInteger("跳过思考次数", "", "参数", DesignerProperty.DisplayMode.Parameter, 1, DesignerProperty.DesignerFlags.NoFlags, null, 1, 100000, 1, "")]
        public int ThinkCount
        {
            get { return thinkCount; }
            set { thinkCount = value; }
        }
#endif

#if AIMind
        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            MoveByForwardInThinkCountNode node = (MoveByForwardInThinkCountNode)newnode;
            node.ThinkCount = thinkCount;
        }

        public MoveByForwardInThinkCountNode() : base("往前走并跳过思考", "") { }
#else
        public MoveByForwardInThinkCountNode(int ThinkCount)
        {
            thinkCount = ThinkCount;
        }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.MoveByForwardInThinkCount(thinkCount);
        }
#endif
    }
}
