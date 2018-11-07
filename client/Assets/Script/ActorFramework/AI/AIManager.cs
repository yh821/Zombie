using AIMind;
using AIRuntime;
using UnityEngine;

public interface IAIManager : IAIProc
{
    void Active();
    void Deactive();
    void TryThink();

    string AIType { get; set; }
    int ThinkInterval { get; set; }
}

public abstract class AIManager : IAIManager
{
    #region 变量
    protected Actor owner;

    protected BTNode AIRoot
    {
        get { return aiRoot; }
        set
        {
            //if (value != aiRoot)
            //    SetAllAINode(value);

            aiRoot = value;
        }
    }
    protected BTNode aiRoot = null;

    public string AIType
    {
        get { return aiType; }
        set { aiType = value; }
    }
    protected string aiType = "";

    public bool IsActive { get; protected set; }

    public bool IsThinking { get; protected set; }

    public int ThinkInterval
    {
        get { return thinkInterval; }
        set { thinkInterval = value; }
    }
    protected int thinkInterval = 0;
    protected uint timerID = 0;
    protected System.Object thisLock = new System.Object();

    protected int MoveSkipThinkCount { get; set; }
    protected float MoveSkipTime { get; set; }
    #endregion

    #region 内部函数
    public AIManager(Actor theOwner)
    {
        owner = theOwner;
    }

    protected virtual void Think()
    {
        lock (thisLock)
        {
            if (!IsActive)
                return;

            if (!IsThinking)
                return;

            AIRoot.Proc(this);
        }
    }

    protected virtual void GetAIDataByType()
    {
    }

    protected BTNode LoadBT(string type)
    {
        string fileName = "BT_" + type;
        BTNode btNode = BTLoader.GetBehaviorTree(fileName);
        if (btNode != null)
            return btNode;

        TextAsset textAsset = CResourceSys.Instance.LoadBehaviourTree(fileName);
        if (textAsset == null)
            return null;

        btNode = BTLoader.LoadBtXml(fileName, textAsset.text);
        Resources.UnloadAsset(textAsset);
        if (btNode == null)
            Debug.LogError("创建BTree失败 " + fileName);
        return btNode;
    }
    #endregion

    #region 外部调用
    public virtual void Active()
    {
        if (IsActive)
            return;
        CTimeSys.Instance.DelTimer(timerID);
        GetAIDataByType();

        AIRoot = LoadBT(AIType);
        if (AIRoot == null)
            return;

        IsActive = true;
        IsThinking = true;

        //owner.GetConditionManager().RegistThinkCondition(GetAIConditionMessage(AIRoot));

        timerID = CTimeSys.Instance.AddTimer(0, ThinkInterval, Think);
    }

    public virtual void Deactive()
    {
        if (!IsActive)
            return;

        IsActive = false;
        IsThinking = false;

        CTimeSys.Instance.DelTimer(timerID);

        if (null != owner)
            owner.StopMoveToPoint();
    }

    public void OnDestroy()
    {
        CTimeSys.Instance.DelTimer(timerID);
        owner = null;
    }

    public void PauseThink()
    {
        if (!IsActive)
            return;

        IsThinking = false;

        //if (null != owner)
        //    owner.StopAIMove();
    }

    public void ResumeThink()
    {
        if (!IsActive)
            return;

        IsThinking = true;
    }

    public void TryThink()
    {
        if (!IsActive)
            return;

        Think();
    }
    #endregion

    #region AI接口

    #region 条件节点
    public bool CheckSelf()
    {
        if (owner.IsDead)
            return false;

        if (MoveSkipThinkCount > 0)
        {
            MoveSkipThinkCount--;
            return false;
        }

        if (MoveSkipTime > 0)
        {
            MoveSkipTime -= ThinkInterval * 0.001f;
            return false;
        }

        return true;
    }

    public bool CheckRandom(int random)
    {
        return UnityEngine.Random.Range(1, 101) < random;
    }

    public bool FindAttackTarget()
    {
        if (null == owner)
            return false;

        return owner.FindAttackTarget();
    }

    public bool FindFollowTarget()
    {
        if (null == owner)
            return false;

        return owner.FindFollowTarget();
    }

    public bool CheckAttackRange()
    {
        if (null == owner)
            return false;

        return owner.CheckAttackRange();
    }

    public bool TooFarFollowTarget()
    {
        if (null == owner)
            return false;

        return owner.TooFarFollowTarget();
    }
    #endregion

    #region 行为节点
    public bool MoveToPoint(float x, float z, float range)
    {
        if (null == owner)
            return false;

        return owner.MoveToPoint(new Vector3(x, owner.transform.position.y, z));
    }

    public bool StopMoveTo()
    {
        if (null == owner)
            return false;

        return owner.StopMoveToPoint();
    }

    public bool StareBlanklyInThinkCount(int thinkCount)
    {
        if (null == owner)
            return false;

        MoveSkipThinkCount = thinkCount;
        owner.StopMoveToPoint();
        return true;
    }

    public bool MoveByForwardInThinkCount(int thinkCount)
    {
        if (null == owner)
            return false;

        MoveSkipThinkCount = thinkCount;
        return owner.MoveToPoint(owner.transform.position + owner.transform.forward * 3);
    }

    public bool TurnToRandomDir(float angle1, float angle2)
    {
        if (null == owner)
            return false;

        owner.transform.forward = Quaternion.Euler(owner.transform.eulerAngles.x,
            owner.transform.eulerAngles.y + UnityEngine.Random.Range(angle1, angle2),
            owner.transform.eulerAngles.z) * Vector3.forward;
        return true;
    }

    public bool ChangeAction(string actStr)
    {
        if (null == owner)
            return false;

        owner.SetTrigger(actStr);
        return true;
    }

    public bool MoveToFollowTarget()
    {
        if (null == owner)
            return false;

        return owner.MoveToFollowTarget();
    }

    public bool MoveToAttackTarget()
    {
        if (null == owner)
            return false;

        return owner.MoveToAttackTarget();
    }

    public bool AttackTheTarget()
    {
        return owner.AttackTheTarget();
    }
    #endregion

    #endregion
}
