using System.Collections.Generic;
using GameData;
using UnityEngine;

/// <summary>
/// 热兵器类
/// 1.所有热兵器暂时统一子弹模型
/// 2.所有子弹统一速度
/// </summary>
public class Gun : Weapon
{
    public Gun(ResSkill data) : base(data)
    {
    }

    public override void ShowEffect()
    {
        if (Owner != null && Owner is Hero)
        {
            if (GamingSys.Instance.Player.Bullet > 0)//子弹数判断
            {
                GameObject cCastEffect = EffectManager.Create("GenericMuzzleFlash", true, (Owner as Hero).FirePoint);
                if (null != cCastEffect)
                {
                    cCastEffect.transform.forward = Owner.transform.forward;
                }

                GameObject go = EffectManager.Create("Z_bullet", false);
                go.transform.position = (Owner as Actor).FirePoint.position - (Owner as Actor).FirePoint.forward;
                FlyBullet bullet = go.AddUniqueComponent<FlyBullet>();
                bullet.Skill = this;
                bullet.Forward =
                    XUtility.DirectionNoY((Owner as Actor).AttackTarget.position - go.transform.position).normalized;
                bullet.LayerMask = mCollisionMask ^ (1 << Owner.gameObject.layer);
                bullet.EnterScene();
                GamingSys.Instance.Player.Bullet--;
            }
            else
            {
                GamingSys.Instance.Player.ChangeWeapon();
            }
        }
    }
}
