using System.Collections;
using System.Collections.Generic;
using GameData;
using UnityEngine;

public enum EActorType
{
    Player = 0,
    Hero = 1,
    Monster,
    GunItem,
    ExpendItem,
}

public class ActorManager : CGameSystem
{
    #region 变量
    public static ActorManager Instance { get; private set; }
    public Dictionary<long, ActorObject> AllActors { get { return mAllActors; } }
    private Dictionary<long, ActorObject> mAllActors = new Dictionary<long, ActorObject>();
    public Dictionary<long, Monster> EnemyDict { get { return mEnemyDict; } }
    private Dictionary<long, Monster> mEnemyDict = new Dictionary<long, Monster>();
    public Dictionary<long, Hero> TeamDict { get { return mTeamDict; } }
    private Dictionary<long, Hero> mTeamDict = new Dictionary<long, Hero>();
    public Dictionary<long, ItemObject> ItemDict { get { return mItemDict; } }
    private Dictionary<long, ItemObject> mItemDict = new Dictionary<long, ItemObject>();

    public static Transform HeroRoot
    {
        get
        {
            if (null == mHeroRoot)
            {
                GameObject objRoot = new GameObject();
                objRoot.name = "HeroRoot";

                mHeroRoot = objRoot.transform;
                mHeroRoot.position = Vector3.zero;
                mHeroRoot.localScale = Vector3.one;
            }

            return mHeroRoot;
        }
    }
    private static Transform mHeroRoot;

    public static Transform MonsterRoot
    {
        get
        {
            if (null == mMonsterRoot)
            {
                GameObject objRoot = new GameObject();
                objRoot.name = "MonsterRoot";

                mMonsterRoot = objRoot.transform;
                mMonsterRoot.position = Vector3.zero;
                mMonsterRoot.localScale = Vector3.one;
            }

            return mMonsterRoot;
        }
    }
    private static Transform mMonsterRoot;

    public static Transform ItemRoot
    {
        get
        {
            if (null == mItemRoot)
            {
                GameObject objRoot = new GameObject();
                objRoot.name = "ItemRoot";

                mItemRoot = objRoot.transform;
                mItemRoot.position = Vector3.zero;
                mItemRoot.localScale = Vector3.one;
            }

            return mItemRoot;
        }
    }
    private static Transform mItemRoot;
    #endregion

    #region override
    public override void SysInitial()
    {
        base.SysInitial();
        Instance = this;
    }

    public override bool SysEnter()
    {
        base.SysEnter();
        return false;
    }

    public override void SysLeave()
    {
        base.SysLeave();

        mAllActors.Clear();
        mTeamDict.Clear();
        mEnemyDict.Clear();
    }
    #endregion

    #region 外部调用
    public static void Cache(EActorType type, int nResId, int nCount)
    {
        for (int i = 0; i < nCount; ++i)
        {
            Cache(type, nResId);
        }
    }

    public static void Cache(EActorType type, int nResId)
    {
        string strRolePrefab = null;
        switch (type)
        {
            case EActorType.Hero:
                ResActor resHero = DataReader<ResActor>.Get(nResId);
                if (null == resHero)
                {
                    Debug.LogError("Error Hero ID = " + nResId);
                    return;
                }
                strRolePrefab = resHero.prefab;
                break;

            case EActorType.Monster:
                ResActor resMonster = DataReader<ResActor>.Get(nResId);
                if (null == resMonster)
                {
                    Debug.LogError("Error Hero ID = " + nResId);
                    return;
                }
                strRolePrefab = resMonster.prefab;
                break;
        }

        Object cPrefab = CResourceSys.Instance.LoadRole(strRolePrefab);
        if (null == cPrefab)
        {
            Debug.LogError("can't load prefabs id:" + nResId);
            return;
        }

        GameObject objRole = ObjectPoolSys.Instance.Cache(cPrefab);
        objRole.transform.parent = HeroRoot;
        objRole.transform.localScale = Vector3.one;
        objRole.transform.localPosition = Vector3.zero;
        objRole.transform.localEulerAngles = Vector3.zero;
    }

    public SceneObject Create(int id, Vector3 pos)
    {
        ResActor data = DataReader<ResActor>.Get(id);
        if (data == null)
            return null;
        Object cPrefab;
        GameObject model;
        SceneObject obj = null;
        switch ((EActorType)data.type)
        {
            case EActorType.Hero:
                cPrefab = CResourceSys.Instance.LoadRole(data.prefab);
                if (cPrefab == null)
                    return null;
                model = ObjectPoolSys.Instance.Create(cPrefab);
                obj = model.AddUniqueComponent<Hero>();
                obj.transform.parent = HeroRoot;
                (obj as Hero).position = pos;
                (obj as Hero).Data = data;
                obj.EnterScene();
                break;

            case EActorType.Monster:
                cPrefab = CResourceSys.Instance.LoadRole(data.prefab);
                if (cPrefab == null)
                    return null;
                model = ObjectPoolSys.Instance.Create(cPrefab);
                obj = model.AddUniqueComponent<Monster>();
                obj.transform.parent = MonsterRoot;
                (obj as Monster).position = pos;
                (obj as Monster).Data = data;
                obj.EnterScene();
                break;

            case EActorType.GunItem:
                cPrefab = CResourceSys.Instance.LoadItem(data.prefab);
                if (cPrefab == null)
                    return null;
                model = ObjectPoolSys.Instance.Create(cPrefab);
                obj = model.AddUniqueComponent<GunItem>();
                obj.transform.parent = ItemRoot;
                obj.transform.position = pos;
                (obj as GunItem).SetSkill(data.skills[0]);
                obj.EnterScene();
                break;

            case EActorType.ExpendItem:
                cPrefab = CResourceSys.Instance.LoadItem(data.prefab);
                if (cPrefab == null)
                    return null;
                model = ObjectPoolSys.Instance.Create(cPrefab);
                obj = model.AddUniqueComponent<ExpendItem>();
                obj.transform.parent = ItemRoot;
                obj.transform.position = pos;
                (obj as ExpendItem).SetSkill(data.skills[0]);
                obj.EnterScene();
                break;
        }

        return obj;
    }
    #endregion
}
