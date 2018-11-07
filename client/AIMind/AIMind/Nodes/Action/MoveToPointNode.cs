//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class MoveToPointNode : Action
    {
        protected float x = 0;
        protected float z = 0;
        protected float range = 0;

#if AIMind
        [DesignerFloat("X(m)", "", "参数", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, "", -100000.0f, 100000.0f, 1, 2, "")]
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        [DesignerFloat("Z(m)", "", "参数", DesignerProperty.DisplayMode.Parameter, 1, DesignerProperty.DesignerFlags.NoFlags, "", -100000.0f, 100000.0f, 1, 2, "")]
        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        [DesignerFloat("停止范围(m)", "", "参数", DesignerProperty.DisplayMode.Parameter, 2, DesignerProperty.DesignerFlags.NoFlags, "", 0.0f, 1000.0f, 1, 2, "")]
        public float Range
        {
            get { return range; }
            set { range = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            MoveToPointNode node = (MoveToPointNode)newnode;
            node.X = x;
            node.Z = z;
            node.Range = range;
        }

        public MoveToPointNode() : base("移动向点", "移动到指定的点") { }
#else
        public MoveToPointNode(float X, float Z, float Range)
        {
            x = X;
            z = Z;
            range = Range;
        }

        public override bool Proc(IAIProc theOwner)
        {
            return theOwner.MoveToPoint(x, z, range);
        }
#endif
    }
}
