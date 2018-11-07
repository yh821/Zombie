using System;
using UnityEngine;

namespace XEngine
{
    /// <summary>
    /// 实际是一个从底层动画往上抛的事件，通用来说并不关心是否有人接收，或者谁接收，
    /// 所以尝试使用新版的Messaging System去实现这个类
    /// </summary>
    public class ActorEventReceiver : MonoBehaviour
    {
        #region 变量

        protected Actor m_actor = null;
        protected Actor actor
        {
            get
            {
                if (null == m_actor)
                {
                    Transform temp = transform;
                    while (temp)
                    {
                        m_actor = temp.GetComponent<Actor>();

                        if (null != m_actor)
                            return m_actor;
                        else
                            temp = temp.parent;
                    }
                }

                return m_actor;
            }
        }

        protected Transform m_root = null;
        protected Transform root
        {
            get
            {
                if (!m_root)
                    m_root = null != actor ? actor.transform : transform.parent;

                return m_root;
            }
        }

        protected Animator m_animator;

        protected Vector3 tempForward;
        protected Vector3 deltaMove;

        #endregion

        #region Init

        void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        #endregion

        #region AnimationEvent

        #region 基本帧

        // 动作自然开始
        public void OnActionStart(AnimationEvent e)
        {
            if (null != actor)
                actor.OnActionStart(e);
        }

        // 动作自然结束
        public void OnActionEnd(AnimationEvent e)
        {
            if (null != actor)
                actor.OnActionEnd(e);
            //if (!CheckEventIsValid(e))
            //    return;

            //CommandCenter.ExecuteCommand(root, new AnimationEndCmd()
            //{
            //    actName = e.stringParameter
            //});
        }

        // 动作重复开始
        public void RepeatStart(AnimationEvent e)
        {
        }

        // 动作重复结束
        public void RepeatEnd(AnimationEvent e)
        {
        }

        #endregion

        #region 战斗相关

        public void OnSkillEffect(AnimationEvent e)
        {
            if (null != actor)
                actor.OnSkillEffect(e);
            //if (!CheckEventIsValid(e))
            //    return;

            //CommandCenter.ExecuteCommand(root, new SkillEffectCmd()
            //{
            //    args = e
            //});
        }

        #endregion

        #region 辅助逻辑

        protected bool CheckEventIsValid(AnimationEvent e)
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).Equals(e.animatorStateInfo) &&
                !m_animator.IsInTransition(0);
        }

        #endregion

        #endregion
    }
}