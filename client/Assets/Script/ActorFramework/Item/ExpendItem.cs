using Attr;
using GameData;

public class ExpendItem : ItemObject
{
    public override void BePickup(ActorObject actor)
    {
        ResSkill data = DataReader<ResSkill>.Get(SkillId);
        if (data == null)
            return;
        switch ((Skill.ESkillType)data.type)
        {
            case Skill.ESkillType.Bullet:
                GamingSys.Instance.Player.Bullet += data.bullet;
                break;
            case Skill.ESkillType.Food:
                GamingSys.Instance.Player.Food += data.food;
                break;
            case Skill.ESkillType.HpPack:
                GamingSys.Instance.Player.AddHp(data.hp);
                break;
            default:
                actor.Attr.Add(new SkillAttr(data));
                break;
        }
        LeaveScene();
    }
}

