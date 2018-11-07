//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class TurnToRandomDirNode : Action
    {
        protected float angle1 = 0;
        protected float angle2 = 0;

#if AIMind       
        [DesignerFloat("角度1", "随机角度范围", "参数", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, "", -360.0f, 360.0f, 1, 2, "")]
        public float Angle1
        {
            get { return angle1; }
            set { angle1 = value; }
        }

        [DesignerFloat("角度2", "随机角度范围", "参数", DesignerProperty.DisplayMode.Parameter, 1, DesignerProperty.DesignerFlags.NoFlags, "", -360.0f, 360.0f, 1, 2, "")]
        public float Angle2
        {
            get { return angle2; }
            set { angle2 = value; }
        }

#endif

#if AIMind
        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            TurnToRandomDirNode node = (TurnToRandomDirNode)newnode;
            node.Angle1 = angle1;
            node.Angle2 = angle2;
        }
        public TurnToRandomDirNode() : base("转向随机角度", "") { }
#else
        public TurnToRandomDirNode(float Angle1, float Angle2)
        {
            angle1 = Angle1;
            angle2 = Angle2;
        }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.TurnToRandomDir(angle1, angle2);
        }
#endif
    }
}
