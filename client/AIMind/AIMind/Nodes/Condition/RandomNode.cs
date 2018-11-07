//#define AIMind
#if AIMind
using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
#else
using AIRuntime;
#endif

namespace AIMind.Nodes
{
    public class RandomNode : ConditionConnectors
    {
        protected int mRandom = 0;
#if AIMind
        [DesignerInteger("概率1-100", "", "参数", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, null, 0, 100, 1, "")]
        public int Random
        {
            get { return mRandom; }
            set { mRandom = value; }
        }
#endif
#if AIMind
        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            RandomNode node = (RandomNode)newnode;
            node.Random = mRandom;
        }
        public RandomNode() : base("是否概率中", "") { }
#else
        public RandomNode(int Random)
        {
            mRandom = Random;
        }

        public override bool Condition(IAIProc theOwner)
        {
            return theOwner.CheckRandom(mRandom);
        }
#endif
    }
}
