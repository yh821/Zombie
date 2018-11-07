using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attr
{
    public enum AttrType : uint
    {
        /// <summary>
        /// 等级
        /// </summary>
        Level = 100,

        /// <summary>
        /// 经验值
        /// </summary>
        Exp,

        /// <summary>
        /// 移动速度
        /// </summary>
        MoveSpeed,

        /// <summary>
        /// 当前生命
        /// </summary>
        Hp,

        /// <summary>
        /// 生命上限
        /// </summary>
        HpLmt,

        /// <summary>
        /// 攻击
        /// </summary>
        Atk,

        /// <summary>
        /// 防御
        /// </summary>
        Defence,

        /// <summary>
        /// 攻击距离
        /// </summary>
        AttackDist,

        /// <summary>
        /// 攻击间隔
        /// </summary>
        AtkDuration,

        /// <summary>
        /// 发现范围
        /// </summary>
        FindDist,

        /// <summary>
        /// 丢失范围
        /// </summary>
        LostDist,

        /// <summary>
        /// 食物
        /// </summary>
        Food,

        /// <summary>
        /// 子弹
        /// </summary>
        Bullet,

        /// <summary>
        /// 命中率
        /// </summary>
        HitRatio,

        /// <summary>
        /// 闪避率
        /// </summary>
        DodgeRatio,

        /// <summary>
        /// 暴击率
        /// </summary>
        CritRatio,

        /// <summary>
        /// 抗暴率
        /// </summary>
        DecritRatio,
    }
}
