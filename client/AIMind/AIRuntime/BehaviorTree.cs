using System.Collections.Generic;
using AIMind;

namespace AIRuntime
{
    public abstract class Command
    {
        public abstract void AddChild(Command root);
        public abstract bool Proc(IAIProc theOwner);
    }

    public class BTNode : Command
    {
        public Command root = null;

        public override bool Proc(IAIProc theOwner)
        {
            return root.Proc(theOwner);
        }

        public override void AddChild(Command root)
        {
            this.root = root;
        }
    }

    public abstract class CompositeNode : Command
    {
        protected List<Command> children = new List<Command>();

        public override bool Proc(IAIProc theOwner)
        {
            return true;
        }

        public override void AddChild(Command node)
        {
            children.Add(node);
        }

        public void DelChild(Command node)
        {
            children.Remove(node);
        }

        public void ClearChildren()
        {
            children.Clear();
        }

        public List<Command> GetChild()
        {
            return children;
        }
    }

    public class Decorator : Command
    {
        protected Command child = null;

        public override bool Proc(IAIProc theOwner)
        {
            return true;
        }

        public override void AddChild(Command node)
        {
            child = node;
        }
    }

    public class Action : Command
    {
        public override bool Proc(IAIProc theOwner)
        {
            return false;
        }

        public override void AddChild(Command _node)
        {
        }
    }

    public class Selector : CompositeNode
    {
        public override bool Proc(IAIProc theOwner)
        {
            for (int index = 0; index < children.Count; ++index)
            {
                if (children[index].Proc(theOwner))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class Sequence : CompositeNode
    {
        public override bool Proc(IAIProc theOwner)
        {
            for (int index = 0; index < children.Count; ++index)
            {
                if (!children[index].Proc(theOwner))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class Parallel : CompositeNode
    {
        private bool _result;
        public override bool Proc(IAIProc theOwner)
        {
            _result = true;

            for (int index = 0; index < children.Count; ++index)
            {
                if (!children[index].Proc(theOwner))
                {
                    _result = false;
                }
            }

            return _result;
        }
    }

    public class ConditionConnectors : Command
    {
        public Command TrueChild = null;
        public Command FalseChild = null;

        public sealed override bool Proc(IAIProc theOwner)
        {
            bool result = Condition(theOwner);
            if (result && TrueChild != null)
                return TrueChild.Proc(theOwner);
            else if (!result && FalseChild != null)
                return FalseChild.Proc(theOwner);
            return false;
        }

        public sealed override void AddChild(Command _node)
        {
        }

        public virtual bool Condition(IAIProc theOwner)
        {
            return false;
        }
    }
}