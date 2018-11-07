using System.Collections.Generic;
using System.Linq;
using Attr;
using GameData;
using UnityEngine;

public abstract class ActorObject : SceneObject
{
    #region 变量

    #region 绑点
    public Transform HurtPoint;
    public Transform WeaponPoint;
    public Transform FirePoint;
    #endregion

    /// <summary>
    /// 配表数据
    /// </summary>
    public ResActor Data { get; set; }
    public SkillAttr Attr { get; protected set; }

    /// <summary>
    /// 技能，至少一个
    /// </summary>
    public Skill CurSkill
    {
        get { return mCurSkill; }
        protected set
        {
            if (mCurSkill != null)
            {
                Attr.Minus(mCurSkill.Attr);
            }
            mCurSkill = value;
            if (mCurSkill != null)
            {
                Attr.Add(mCurSkill.Attr);
                SetInteger("weapon_type", mCurSkill.Data.type);
            }
            else
            {
                SetInteger("weapon_type", 0);
            }
        }
    }
    private Skill mCurSkill = null;
    protected Dictionary<int, Skill> mSkills = new Dictionary<int, Skill>();

    //public Dictionary<int, ItemObject> Pack { get { return mPack; } }
    protected Dictionary<int, ItemObject> mPack = new Dictionary<int, ItemObject>();

    private List<ResExp> mExpDataList;
    #endregion

    #region override
    public override void OnCreate()
    {
        base.OnCreate();
        Attr = new SkillAttr();
        mExpDataList = DataReader<ResExp>.DataList;
    }

    public override void EnterScene()
    {
        base.EnterScene();

        if (Data != null)
        {
            Attr.Exp = Data.exp;
            SetLevel(Data.level);
            for (int i = 0; i < Data.skills.Count; i++)
                AddSkill(Data.skills[i]);
            for (int i = 0, len = Data.items.Count; i < len; i++)
            {
                ItemObject item = ActorManager.Instance.Create(Data.items[i], transform.position) as ItemObject;
                if (item != null)
                    item.BePickup(this);
            }
        }
    }

    public override void LeaveScene()
    {
        base.LeaveScene();

        mSkills.Clear();
        CurSkill = null;
        Attr.Clear();
        Attr.Level = 0;
        Attr.Exp = 0;
        Attr.Hp = 0;
    }

    public override void OnDelete()
    {
        mSkills.Clear();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        foreach (var skill in mSkills.Values)
            skill.Update();
    }
    #endregion

    #region 外部调用
    public void AddItem(ItemObject newItem)
    {
        ResSkill skillData = DataReader<ResSkill>.Get(newItem.SkillId);
        if (skillData == null)
            return;

        ItemObject oldItem = null;
        if (mPack.TryGetValue(skillData.kind, out oldItem))
        {
            //品质好过背包
            if (mSkills[oldItem.SkillId].Data.level < skillData.level)
                GiveUpItem(oldItem);//扔掉背包里的武器
            else
                return;
        }

        //拾取地上的武器
        mPack[skillData.kind] = newItem;
        PickUpItem(newItem);
        FirePoint = newItem.transform.GetComponent<PointHandle>().firePoint;
        if (skillData.kind == GamingSys.Instance.Player.WeaponKind)
        {
            newItem.gameObject.SetActive(true);
            AddSkill(skillData, true);//添加技能
        }
        else
        {
            newItem.gameObject.SetActive(false);
            AddSkill(skillData, false);//添加技能
        }
    }

    public void ClearItem()
    {
        foreach (var item in mPack.Values)
        {
            GiveUpItem(item);
            RemoveSkill(item.SkillId);
        }
        mPack.Clear();
    }

    public void SwitchItem(int newKind, int oldKind)
    {
        ItemObject item = null;
        if (mPack.TryGetValue(newKind, out item))
        {
            item.gameObject.SetActive(true);
            CurSkill = mSkills[item.SkillId];
        }
        else//默认技能
        {
            CurSkill = mSkills.Values.First();
        }

        if (mPack.TryGetValue(oldKind, out item))
        {
            item.gameObject.SetActive(false);
        }
    }

    public Skill AddSkill(int skillId, bool isCurrent = false)
    {
        return AddSkill(DataReader<ResSkill>.Get(skillId));
    }

    public Skill AddSkill(ResSkill data, bool isCurrent = false)
    {
        Skill skill = null;
        if (data != null)
        {
            if (mSkills.ContainsKey(data.id))
            {
                if (Skill.Create(data, out skill))
                    mSkills[data.id] = skill;
                else
                    return null;
            }
            else if (Skill.Create(data, out skill))
            {
                mSkills.Add(data.id, skill);
            }

            if (skill != null)
            {
                skill.SetOwner(this);
                if (CurSkill == null || isCurrent)
                    CurSkill = skill;
            }
        }
        return skill;
    }

    public void RemoveSkill(int id)
    {
        mSkills.Remove(id);
        if (CurSkill.ID == id)
            CurSkill = mSkills.Values.First();
    }

    public void AddExp(long exp)
    {
        Attr.Exp += exp;
        ResExp data = mExpDataList.Find(e => e.max >= Attr.Exp && e.min <= Attr.Exp);
        if (data != null && data.level > Attr.Level)
        {
            SetLevel(data);
            EventDispatcher.Broadcast(EventNames.LevelUp, this);
        }
    }
    #endregion

    #region 内部调用
    protected void GiveUpItem(ItemObject item)
    {
        Vector3 newDire = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * transform.forward;
        Vector3 newPos = transform.position + newDire;
        item.transform.parent = ActorManager.ItemRoot;
        item.transform.forward = newDire;
        item.transform.position = newPos;
        item.transform.GetComponent<BoxCollider>().enabled = true;
        item.gameObject.SetActive(true);
    }

    protected void PickUpItem(ItemObject item)
    {
        item.transform.parent = WeaponPoint;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.GetComponent<BoxCollider>().enabled = false;
    }

    protected void SetLevel(int level)
    {
        SetLevel(DataReader<ResExp>.Get(level));
    }

    protected void SetLevel(ResExp data)
    {
        if (data != null)
        {
            Attr.Clear();
            Attr.MoveSpeed = Data.speed;
            Attr.Level = data.level;
            Attr.HpLmt = data.hp_lmt;
            Attr.Defence = data.defence;
            Attr.Atk = data.attack;
            if (CurSkill != null)
                Attr.Add(CurSkill.Attr);
            Attr.Hp = Attr.HpLmt;
        }
    }
    #endregion
}
