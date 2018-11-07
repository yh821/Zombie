using Attr;
using Base;
using GameData;

public interface ISkill
{
    void SetOwner(ActorObject actor);
    void Execute();
    void ShowEffect();
    void Update();
}

public abstract class Skill : ISkill
{
    public enum ESkillType
    {
        Fist = 101,//拳头
        Cold = 102,//冷兵器
        Pistol = 103,//手枪
        Rifle = 104,//步枪
        Weapon,//武器分界线==============

        Helmet = 201,//头盔
        Armor,//防具分界线===============

        Bullet = 301,//子弹
        Food,//食物
        HpPack,//药包
        Item,//道具分界线================

        Heal = 401,//治疗buff
        Buff,//BUFF分界线================
    }

    public enum ESkillKind
    {
        Cold = 1,//近战
        Hot = 2,//热兵器
    }

    public int ID { get; protected set; }
    public ActorObject Owner { get; protected set; }
    public SkillAttr Attr { get; protected set; }
    public ResSkill Data { get; protected set; }

    protected Skill(ResSkill data)
    {
        Data = data;
        ID = Data.id;

        Attr = new SkillAttr(Data);
    }

    public virtual void SetOwner(ActorObject owner)
    {
        Owner = owner;
    }

    public abstract void Execute();

    public abstract void ShowEffect();

    public abstract void Update();

    public static bool Create(ResSkill data, out Skill skill)
    {
        if (data != null)
        {
            switch ((ESkillType)data.type)
            {
                case ESkillType.Fist:
                case ESkillType.Cold:
                    skill = new Weapon(data);
                    return true;
                case ESkillType.Pistol:
                case ESkillType.Rifle:
                    skill = new Gun(data);
                    return true;
                case ESkillType.Heal:
                    skill = new Buff(data);
                    return true;
            }
        }
        skill = null;
        return false;
    }
}
