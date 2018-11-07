using UnityEngine;

namespace XEngine
{
    #region 基本帧

    public class AnimationStartCmd : BaseCommand
    {
        public string actName;
    }

    public class AnimationEndCmd : BaseCommand
    {
        public string actName;
    }

    public class RepeatStartCmd : BaseCommand
    {
        public string actName;
        public float time;
    }

    public class RepeatEndCmd : BaseCommand
    {
        public string actName;
    }

    #endregion

    #region 战斗相关

    public class SkillEffectCmd : BaseCommand
    {
        public AnimationEvent args;
    }

    public class AddSkillCmd : BaseCommand
    {
        public string skillMessage;
    }

    public class RemoveSkillCmd : BaseCommand
    {
        public string skillMessage;
    }

    public class ActivePartCmd : BaseCommand
    {
        public AnimationEvent args;
    }

    public class DeactivePartCmd : BaseCommand
    {
        public AnimationEvent args;
    }

    public class ChangeAICmd : BaseCommand
    {
        public AnimationEvent args;
    }

    public class CheckComboCmd : BaseCommand
    {
    }

    public class SetTerminationCmd : BaseCommand
    {
        public string actionName;
    }

    public class AllowChangeSpeedCmd : BaseCommand
    {
    }

    public class NotifyPropChangedCmd : BaseCommand
    {
        public string propName;
        public float propValue;
        public string propTag;
    }

    public class AllowChangeDirectionCmd : BaseCommand
    {
    }

    public class LockFaceForwardCmd : BaseCommand
    {
    }

    public class RootMotionCmd : BaseCommand
    {
        public bool rootMotion;
    }

    public class IgnoreCollisionCmd : BaseCommand
    {
        public bool closeCollision;
    }

    public class ActionStraightCmd : BaseCommand
    {
        public float rate = 1.0f;
    }

    #endregion
}