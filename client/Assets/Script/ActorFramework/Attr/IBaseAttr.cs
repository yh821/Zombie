using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attr
{
    interface IBaseAttr
    {
        int Level { get; set; }
    }

    interface IActorAttr : IBaseAttr
    {
        long Exp { get; set; }
        float MoveSpeed { get; set; }
        int Hp { get; set; }
        int HpLmt { get; set; }
        int Atk { get; set; }
        int Defence { get; set; }
    }

    interface ISkillAttr : IActorAttr
    {
        float AttackDist { get; set; }
        int AtkDuration { get; set; }
        float FindDist { get; set; }
        float LostDist { get; set; }
        int Food { get; set; }
        int Bullet { get; set; }

        int HitRatio { get; set; }
        int DodgeRatio { get; set; }
        int CritRatio { get; set; }
        int DecritRatio { get; set; }
    }
}
