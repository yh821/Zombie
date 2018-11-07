using System.Collections.Generic;
using GameData;
using UnityEngine;

public class Hero : Actor
{
    #region 变量
    private Vector3 mMoveDire;
    private Vector3 mShotDire;
    private float mCurSpeed;

    private Vector3 mLastPostion;
    private float mDeltaTime;

    public Vector3 FollowPoint = Vector3.zero;
    #endregion

    #region INIT
    public override void OnCreate()
    {
        base.OnCreate();
        mAIMgr = new HeroAI(this);
        mLastPostion = transform.position;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (IsDead)
        {
            AttackTarget = null;
            FollowTarget = null;
            return;
        }

        #region 刷新动作
        if (null != FollowTarget)
        {
            mDeltaTime += Time.deltaTime;
            if (mDeltaTime >= 0.2f)
            {
                mDeltaTime = 0;
                SetFloat("runSpeed", XUtility.DistanceNoY(mLastPostion, transform.position));
                mLastPostion = transform.position;
            }

            if (AttackTarget != null && CurSkill.Data.kind == (int)Skill.ESkillKind.Hot)
            {
                mShotDire = XUtility.DirectionNoY(AttackTarget.transform.position - transform.position);
                if (mShotDire.magnitude != 0)
                {
                    Controller.transform.forward = mShotDire.normalized;
                    mMoveDire = XUtility.DirectionNoY(FollowPoint - transform.position);
                    Vector3 dire = Controller.transform.InverseTransformDirection(mMoveDire.normalized);
                    SetFloat("runDirX", dire.x);
                    SetFloat("runDirZ", dire.z);
                }
            }
        }
        #endregion
    }

    public override void EnterScene()
    {
        base.EnterScene();

        if (!ActorManager.Instance.TeamDict.ContainsKey(ID))
            ActorManager.Instance.TeamDict.Add(ID, this);
    }

    public override void LeaveScene()
    {
        base.LeaveScene();
        if (ActorManager.Instance.TeamDict.ContainsKey(ID))
            ActorManager.Instance.TeamDict.Remove(ID);
    }
    #endregion

    #region AI接口

    #region 条件接口
    public override bool TooFarFollowTarget()
    {
        if (FollowTarget == null)
            return false;

        //if (XUtility.DistanceNoY(FollowTarget.transform.position - transform.position) > Attr.FollowDist)
        //{
        //    return true;
        //}
        //return false;

        return GamingSys.Instance.Player.IsMoving;
    }

    public override bool FindFollowTarget()
    {
        if (null == FollowTarget)
        {
            if (XUtility.DistanceNoY(GamingSys.Instance.Player.transform.position, transform.position) <= CurSkill.Attr.FindDist)
            {
                FollowTarget = GamingSys.Instance.Player;
                GamingSys.Instance.Player.AddTeammate(this);
            }
#if UNITY_EDITOR
            XUtility.DrawCircle(transform.position, transform.forward, CurSkill.Attr.FindDist, Color.green, 1f);
#endif
        }

        return FollowTarget != null;
    }

    public override bool FindAttackTarget()
    {
        AttackTarget = null;
        Dictionary<long, Monster> hero = ActorManager.Instance.EnemyDict;
        float temp, dire = float.MaxValue;
        foreach (Actor actor in hero.Values)
        {
            if (actor.IsDead)
                continue;
            temp = XUtility.DistanceNoY(actor.transform.position, transform.position);
            if (temp <= CurSkill.Attr.FindDist && temp < dire)
            {
                dire = temp;
                AttackTarget = actor;
            }
        }

        if (null != AttackTarget)
        {
            SetBool("findTarget", true);
            //mCurSpeed = Attr.CrouchSpeed;
            return true;
        }
        else
        {
            SetBool("findTarget", false);
            //mCurSpeed = Attr.MoveSpeed;
            return false;
        }
    }
    #endregion

    #region 行为接口
    public override bool MoveToAttackTarget()
    {
        if (AttackTarget == null)
            return false;

        //先暂用网格导航移动方式，后面再加其他移动方式
        Vector3 dire = XUtility.DirectionNoY(transform.position - AttackTarget.transform.position);
        if (dire.magnitude > CurSkill.Attr.AttackDist)
            ControllerNavMeshMove(AttackTarget.transform.position + dire.normalized * CurSkill.Attr.AttackDist);

        return true;
    }

    public override bool MoveToFollowTarget()
    {
        if (FollowTarget == null)
            return false;

        //先暂用网格导航移动方式，后面再加其他移动方式
        ControllerNavMeshMove(FollowPoint);
        return true;
    }
    #endregion

    #endregion

    #region 外部调用
    public override void Dying()
    {
        base.Dying();
        AttackTarget = null;
        FollowTarget = null;
        SetBool("findTarget", false);
        GamingSys.Instance.Player.RemoveTeammate(this.ID);
    }
    #endregion
}
