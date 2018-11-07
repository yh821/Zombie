using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : SceneObject
{
    public bool IsMoving { get; private set; }
    public int WeaponKind { get; private set; }

    public int Bullet
    {
        get { return mBullet; }
        set
        {
            if (mBullet != value)
            {
                mBullet = value;
                EventDispatcher.Broadcast(EventNames.ChangeBullet, value);
            }
        }
    }
    private int mBullet;

    public int Food
    {
        get { return mFood; }
        set
        {
            if (mFood != value)
            {
                mFood = value;
                EventDispatcher.Broadcast(EventNames.ChangeFood, value);
            }
        }
    }
    private int mFood;

    public Dictionary<long, Hero> Teammates { get { return mTeammates; } }
    private Dictionary<long, Hero> mTeammates = new Dictionary<long, Hero>();
    private Dictionary<float, Hero> mDistDict = new Dictionary<float, Hero>();
    private List<float> mDistList;

    private float mCurSpeed;
    private Vector3 mMoveDire;
    private Actor mCaptian = null;

    private float mDeltaTime;
    private bool mIsCoundDown;
    private float mFollowDist;

    private uint mFoodTimeId;

    public override void EnterScene()
    {
        base.EnterScene();

        EventDispatcher.AddListener<Monster>(EventNames.MonstorDying, OnMonsterDying);
        WeaponKind = 1;
        mTeammates.Clear();
        mCurSpeed = 5.2f;
        BeehiveCell.Radius = 0.6f;
        Food = 50;

        mFoodTimeId = CTimeSys.Instance.AddTimer(60000, 60000, EatFood);
    }

    public override void LeaveScene()
    {
        base.LeaveScene();
        EventDispatcher.RemoveListener<Monster>(EventNames.MonstorDying, OnMonsterDying);

        CTimeSys.Instance.DelTimer(mFoodTimeId);
    }

    public void AddTeammate(Hero hero)
    {
        mTeammates.Add(hero.ID, hero);
        while (BeehiveCell.Count < mTeammates.Count)
        {
            BeehiveCell.AddCell();
        }
        SortHeroList();
        mFollowDist = BeehiveCell.Tier * BeehiveCell.Radius * 2;
    }

    public void RemoveTeammate(long heroId)
    {
        mTeammates.Remove(heroId);
        while (BeehiveCell.Count > mTeammates.Count)
        {
            BeehiveCell.RemoveCell();
        }
        SortHeroList();
        mFollowDist = BeehiveCell.Tier * BeehiveCell.Radius * 2;
    }

    public void Move(Vector2 dire)
    {
        mMoveDire = dire;
        float fLen = mCurSpeed * Time.deltaTime;
        Vector3 sOffset = Vector3.zero;

        mMoveDire.Normalize();
        sOffset.x = (mMoveDire.x * fLen);
        sOffset.z = (mMoveDire.y * fLen);
        transform.forward = Quaternion.AngleAxis(-90, Vector3.up) * sOffset;

        IsMoving = true;
        transform.position += sOffset;
    }

    public void Stop()
    {
        IsMoving = false;
        SortHeroList();
    }

    public void ChangeWeapon()
    {
        int newKind = WeaponKind == 1 ? 2 : 1;
        if (newKind == 1 || newKind == 2 && Bullet > 0)
        {
            foreach (var hero in mTeammates.Values)
            {
                hero.SwitchItem(newKind, WeaponKind);
            }
            WeaponKind = newKind;
        }
    }

    private void EatFood()
    {
        Food -= mTeammates.Count;
        if (Food <= 0)
        {
            Food = 0;
            //团灭
        }
    }

    public void AddHp(int hp, bool isSum = true)
    {
        int ehp = 0;
        if (isSum)
            ehp = Mathf.RoundToInt(hp / mTeammates.Count);
        else
            ehp = hp;
        foreach (var hero in mTeammates.Values)
        {
            hero.Attr.Hp += ehp;
            if (hero.Attr.Hp > hero.Attr.HpLmt)
                hero.Attr.Hp = hero.Attr.HpLmt;
        }
    }

    private void SortHeroList()
    {
        mDistDict.Clear();
        float index = 0.1f;
        float dist;
        foreach (Hero hero in mTeammates.Values)
        {
            dist = XUtility.DistanceNoY(hero.transform.position, transform.position);
            if (mDistDict.ContainsKey(dist))
            {
                dist += index;
                index += index;
            }
            mDistDict.Add(dist, hero);
        }
        mDistList = new List<float>(mDistDict.Keys);
        mDistList.Sort(SortDist);
    }

    private int SortDist(float a, float b)
    {
        if (a > b)
            return 1;
        else if (a < b)
            return -1;
        else
            return 0;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!IsMoving)
        {
            mDeltaTime += Time.deltaTime;
            if (mDeltaTime >= 3f)
            {
                mIsCoundDown = true;
                foreach (Hero hero in mTeammates.Values)
                {
                    if (hero.AttackTarget == null
                        && hero.MovingDirection != Vector3.zero
                        && XUtility.DistanceNoY(hero.position, transform.position) <= mFollowDist)
                    {
                        hero.FollowPoint = hero.position;
                        hero.StopMoveToPoint();
                    }
                }
            }
        }
        else
        {
            mIsCoundDown = false;
            mDeltaTime = 0;
        }

        #region 蜂窝站位
        if (mDistList != null && !mIsCoundDown)
        {
            for (int i = 0, len = mDistList.Count; i < len; i++)
            {
                mDistDict[mDistList[i]].FollowPoint = transform.TransformPoint(BeehiveCell.GetBeehivePos(i));
            }
            //mCurSpeed = Mathf.Lerp(Attr.MoveSpeed, 0, Mathf.Max(0, mMaxDist - BeehiveCell.CellTier(mTeammates.Count)) / 10f);
        }
        #endregion

#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Vector3.up * 100, Color.red);
#endif
    }

    private void OnMonsterDying(Monster monster)
    {
        long exp = Mathf.RoundToInt(monster.Attr.Exp / mTeammates.Count);
        foreach (var hero in mTeammates.Values)
            hero.AddExp(exp);
        //Debug.Log(string.Format("add exp <color=green>{0}</color>", exp));
    }
}
