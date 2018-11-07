using GameData;
using UnityEngine;

/// <summary>
/// 武器基类
/// </summary>
public class Weapon : Skill
{
    protected bool mIsCD = false;
    protected static LayerMask mCollisionMask = CLayerDefine.MonstorMask | CLayerDefine.HeroMask;

    public Weapon(ResSkill data) : base(data) { }

    protected virtual void ClearCD()
    {
        mIsCD = false;
    }

    public override void Execute()
    {
        if (!mIsCD)
        {
            mIsCD = true;
            CTimeSys.Instance.AddTimer((uint)Attr.AtkDuration, 0, ClearCD);
            Owner.SetTrigger("attack");
        }
    }

    public override void ShowEffect()
    {
        //todo 创建子弹
        GameObject go = EffectManager.Create("Fist", false);
        go.transform.position = Owner.transform.position;
        go.transform.forward = Owner.transform.forward;
        Bullet bullet = go.AddUniqueComponent<Bullet>();
        bullet.Skill = this;
        bullet.LayerMask = mCollisionMask ^ (1 << Owner.gameObject.layer);
        bullet.EnterScene();
    }

    public override void Update()
    {
    }
}
