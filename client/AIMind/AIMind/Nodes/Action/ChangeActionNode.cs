//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class ChangeActionNode : Action
    {
        protected string actStr = "idle";
#if AIMind
        [DesignerString("动作名", "需要播放的动画名", "参数", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public string ActStr
        {
            get { return actStr; }
            set { actStr = value; }
        }
#endif

#if AIMind
        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ChangeActionNode node = (ChangeActionNode)newnode;
            node.ActStr = actStr;
        }
        public ChangeActionNode() : base("播放动作", "播放指定的动画") { }
#else
        public ChangeActionNode(string ActStr)
        {
            actStr = ActStr;
        }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.ChangeAction(actStr);
        }
#endif
    }
}
