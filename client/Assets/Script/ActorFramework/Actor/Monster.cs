using System.Collections.Generic;
using UnityEngine;

public class Monster : Actor
{
    #region override
    public override void OnCreate()
    {
        base.OnCreate();
        mAIMgr = new MonstorAI(this);
    }

    public override void EnterScene()
    {
        base.EnterScene();
        if (!ActorManager.Instance.EnemyDict.ContainsKey(ID))
            ActorManager.Instance.EnemyDict.Add(ID, this);
    }

    public override void LeaveScene()
    {
        base.LeaveScene();
        if (ActorManager.Instance.EnemyDict.ContainsKey(ID))
            ActorManager.Instance.EnemyDict.Remove(ID);
    }

    public override void Dying()
    {
        base.Dying();
        EventDispatcher.Broadcast(EventNames.MonstorDying, this);
    }
    #endregion

    #region AI接口

    #region 条件接口
    public override bool FindAttackTarget()
    {
        AttackTarget = null;
        Dictionary<long, Hero> hero = GamingSys.Instance.Player.Teammates;
        float temp, dire = float.MaxValue;
        foreach (Actor actor in hero.Values)
        {
            if (actor.IsDead || actor.FollowTarget == null)
                continue;
            temp = XUtility.DistanceNoY(actor.transform.position, transform.position);
            if (temp <= CurSkill.Attr.FindDist && temp < dire)
            {
                dire = temp;
                AttackTarget = actor;
            }
        }
#if UNITY_EDITOR
        XUtility.DrawCircle(transform.position, transform.forward, CurSkill.Attr.FindDist, Color.yellow, 1f);
#endif
        return null != AttackTarget;
    }
    #endregion

    #region 行为接口
    #endregion

    #endregion
}
