using Attr;
using GameData;
using UnityEngine;
using UnityEngine.AI;

public abstract partial class Actor : ActorObject
{
    #region 变量
    /// <summary>
    /// 是否死亡
    /// </summary>
    public bool IsDead { get; protected set; }
    /// <summary>
    /// 是否瘫痪
    /// </summary>
    public bool IsWeak { get; protected set; }
    /// <summary>
    /// 攻击目标
    /// </summary>
    public Actor AttackTarget { get; protected set; }
    /// <summary>
    /// 跟随目标
    /// </summary>
    public SceneObject FollowTarget { get; protected set; }

    public bool HpBarVisable
    {
        get { return mHpBarVisable; }
        set
        {
            if (mHpBarVisable == value)
                return;

            mHpBarVisable = value;
            if (value)
            {
                if (mHpBar == null)
                    mHpBar = UINodesManager.Instance.OpenUI("HpBar", UINodesManager.NoEventsUIRoot).GetComponent<UIHpBar>();
                else
                    mHpBar.gameObject.SetActive(true);
                mHpBar.SetActor(this);
            }
            else if (mHpBar != null)
            {
                mHpBar.gameObject.SetActive(false);
            }
        }
    }
    protected bool mHpBarVisable = false;

    protected AIManager mAIMgr;

    protected UIHpBar mHpBar;

    private uint mTimeId = 0;
    private bool mIsDown = false;
    #endregion

    #region override
    public override void OnCreate()
    {
        base.OnCreate();
        cAnimator = GetComponentInChildren<Animator>();
        Controller = gameObject.AddUniqueComponent<CharacterController>();
        Agent = gameObject.AddUniqueComponent<NavMeshAgent>();
        Agent.updatePosition = false; //关闭NavMeshAgent的自带路径行走
        Agent.updateRotation = false; //关闭NavMeshAgent的自带朝向旋转
        mNavMeshPath = new NavMeshPath();
        mNavMeshHit = new NavMeshHit();

        Attr.AttrIntChangedDelegate = OnAttrChange;
    }

    public override void EnterScene()
    {
        base.EnterScene();
        Controller.enabled = true;
        Agent.enabled = true;
        IsDead = false;
        AttackTarget = null;
        FollowTarget = null;
        mAIMgr.Active();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        MoveProcess();
        UpdateDying();
    }

    public override void LeaveScene()
    {
        base.LeaveScene();
        mAIMgr.Deactive();

        if (mTimeId != 0)
            CTimeSys.Instance.DelTimer(mTimeId);
        mIsDown = false;
    }
    #endregion

    #region 内部调用
    protected void UpdateDying()
    {
        if (IsDead && mIsDown)
        {
            transform.position += Vector3.down / 3 * Time.deltaTime;
        }
    }

    protected void OnAttrChange(AttrType type, int newValue, int oldValue)
    {
        switch (type)
        {
            case AttrType.Hp:
                if (mHpBar != null)
                    mHpBar.Value = (float)Attr.Hp / Attr.HpLmt;
                break;
        }
    }
    #endregion

    #region 外部调用
    public virtual void Dying()
    {
        IsDead = true;
        Controller.enabled = false;
        Agent.enabled = false;
        AttackTarget = null;
        FollowTarget = null;
        SetTrigger("die");

        HpBarVisable = false;//隐藏血条

        ClearItem();//背包掉落

        //滴血
        CTimeSys.Instance.AddTimer(500, 0, () =>
        {
            GameObject go = EffectManager.Create("blood_down", true);
            go.transform.position = transform.position;
            go.transform.forward = transform.forward;
        });

        //血泊
        CTimeSys.Instance.AddTimer(850, 0, () =>
        {
            GameObject go = EffectManager.Create("dimian_canliu", false);
            go.transform.position = transform.position;// + transform.forward * 0.5f;
            go.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
            Renderer blood = go.GetComponentInChildren<Renderer>();
            blood.material.SetFloat("_N", Random.Range(1, 10));
        });

        //3秒后开始下沉
        CTimeSys.Instance.AddTimer(3000, 0, () =>
        {
            mIsDown = true;
            //下沉3秒后离场
            mTimeId = CTimeSys.Instance.AddTimer(3000, 0, LeaveScene);
        });
    }

    public virtual void OnHurt(Skill skill)
    {
        Attr.Hp -= skill.Owner.Attr.Atk * 1000 / Attr.Defence + 100;
        //Debug.Log(Attr.Hp);
        EffectManager.Create("Z_Hit01", true, HurtPoint);
        if (Attr.Hp <= 0)
        {
            Dying();
        }
        else
        {
            HpBarVisable = true;//显示血条
        }
    }
    #endregion

    #region 动画关联
    public virtual void OnActionStart(AnimationEvent e)
    {
    }

    public virtual void OnActionEnd(AnimationEvent e)
    {
    }

    public virtual void OnSkillEffect(AnimationEvent e)
    {
        if (AttackTarget != null)
        {
            CurSkill.ShowEffect();
        }
    }
    #endregion

    #region AI接口

    #region 条件接口
    public virtual bool FindAttackTarget()
    {
        return false;
    }

    public virtual bool FindFollowTarget()
    {
        return false;
    }

    public virtual bool CheckAttackRange()
    {
        if (AttackTarget != null && CurSkill != null
            && XUtility.DistanceNoY(AttackTarget.transform.position - transform.position) <= CurSkill.Attr.AttackDist)
        {
            return true;
        }
        return false;
    }

    public virtual bool TooFarFollowTarget()
    {
        return false;
    }
    #endregion

    #region 行为接口
    public virtual bool AttackTheTarget()
    {
        if (AttackTarget == null)
            return false;

        transform.LookAt(AttackTarget.position);
        CurSkill.Execute();
        return true;
    }

    public virtual bool MoveToPoint(Vector3 point)
    {
        //先暂用网格导航移动方式，后面再加其他移动方式
        ControllerNavMeshMove(point);
        return true;
    }

    public virtual bool StopMoveToPoint()
    {
        IsClearTargetPosition = true;
        MovingDirection = Vector3.zero;
        SetBool("move", false);
        return true;
    }

    public virtual bool MoveToFollowTarget()
    {
        if (FollowTarget == null)
            return false;

        //先暂用网格导航移动方式，后面再加其他移动方式
        ControllerNavMeshMove(FollowTarget.transform.position);
        return true;
    }

    public virtual bool MoveToAttackTarget()
    {
        if (AttackTarget == null)
            return false;

        //先暂用网格导航移动方式，后面再加其他移动方式
        ControllerNavMeshMove(AttackTarget.transform.position);
        return true;
    }
    #endregion

    #endregion
}
