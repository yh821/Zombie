using System.Collections.Generic;
using UnityEngine;

public abstract class ItemObject : SceneObject
{
    public int SkillId { get; protected set; }

    /// <summary>
    /// 是否是消耗道具
    /// </summary>
    public bool IsExpend { get; protected set; }

    public void SetSkill(int skillId)
    {
        //if (skillId == SkillId)
        //{
        //    Debug.Log("重复添加skillId");
        //    return;
        //}
        SkillId = skillId;
    }

    public override void EnterScene()
    {
        base.EnterScene();
        if (!ActorManager.Instance.ItemDict.ContainsKey(ID))
            ActorManager.Instance.ItemDict.Add(ID, this);
    }

    public override void LeaveScene()
    {
        base.LeaveScene();
        if (ActorManager.Instance.ItemDict.ContainsKey(ID))
            ActorManager.Instance.ItemDict.Remove(ID);
    }

    private void OnTriggerEnter(Collider other)
    {
        Hero hero = other.GetComponent<Hero>();
        if (hero != null && !hero.IsDead)
            BePickup(hero);
    }

    /// <summary>
    /// 被拾起
    /// </summary>
    /// <param name="other"></param>
    public abstract void BePickup(ActorObject actor);
}
