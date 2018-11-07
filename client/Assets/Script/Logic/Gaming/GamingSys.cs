using System.Collections.Generic;
using GameData;
using UnityEngine;

public class GamingSys : CGameSystem
{
    public static GamingSys Instance { get { return m_cInstance; } }
    private static GamingSys m_cInstance;

    public Player Player
    {
        set { mPlayer = value; }
        get { return mPlayer; }
    }
    private Player mPlayer = null;

    private List<SceneObject> mMonsters = new List<SceneObject>();

    public int CurTriggerId = 0;

    #region override
    public override void SysInitial()
    {
        base.SysInitial();
        m_cInstance = this;
    }

    public override bool SysEnter()
    {
        EventDispatcher.AddListener<int, BoxCollider>(EventNames.EnterTrigger, OnEnterTrigger);

        GenHeroAndItem();

        //CTimeSys.Instance.AddTimer(10000, 10000, RefreshActor);
        return true;
    }

    private void OnEnterTrigger(int number, BoxCollider box)
    {
        ResScene data = DataReader<ResScene>.Get(number);
        if (data != null)
        {
            foreach (var monster in mMonsters)
                monster.LeaveScene();
            mMonsters.Clear();

            Vector3 pos = box.transform.position;
            Vector3 size = box.size;
            for (int i = 0, len = (number - 200000) + 10; i < len; i++)
            {
                mMonsters.Add(CreateSceneObject(data.actor_id, Random.Range(-size.x, size.x) + pos.x, Random.Range(-size.z, size.z) + pos.z));
            }
        }
    }

    public override void SysLeave()
    {
        base.SysLeave();
        EventDispatcher.RemoveListener<int, BoxCollider>(EventNames.EnterTrigger, OnEnterTrigger);
    }

    public override void SysUpdate()
    {
        DebugUpdate();
    }
    #endregion

    //创建友军
    private void GenHeroAndItem()
    {
        GameObject go = ObjectPoolSys.Instance.Create(CResourceSys.Instance.LoadRole("Player"));
        Player = go.AddUniqueComponent<Player>();
        Player.transform.position = new Vector3(40, 0, 0);
        Player.EnterScene();

        List<ResScene> scenes = DataReader<ResScene>.DataList;
        ResScene scene;
        for (int i = 0; i < scenes.Count; i++)
        {
            scene = scenes[i];
            if (scene.id > 100000 && scene.id < 200000)
            {
                CreateSceneObject(scene.actor_id, scene.pos_x, scene.pos_z);
            }
            else if (scene.id > 200000 && scene.id < 300000)
            {
                //CreateSceneObject(scene.actor_id, scene.pos_x, scene.pos_z);
            }
            else if (scene.id > 300000 && scene.id < 400000)
            {
                CreateSceneObject(scene.actor_id, scene.pos_x, scene.pos_z);
            }
        }
    }

    protected SceneObject CreateSceneObject(int id, float posX, float posZ)
    {
        return ActorManager.Instance.Create(id, new Vector3(posX, 0, posZ));
    }

    private void DebugUpdate()
    {
#if UNITY_EDITOR
        if ((Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.F)
            || Input.GetKeyUp(KeyCode.R) || Input.GetKeyUp(KeyCode.T)))
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                int id = 0;
                if (Input.GetKeyUp(KeyCode.E))
                    id = 2001;
                else if (Input.GetKeyUp(KeyCode.Q))
                    id = 1004;
                else if (Input.GetKeyUp(KeyCode.R))
                    id = 4002;
                else if (Input.GetKeyUp(KeyCode.T))
                    id = 4010;
                else if (Input.GetKeyUp(KeyCode.F))
                    id = 4005;

                CreateSceneObject(id, hitInfo.point.x, hitInfo.point.z);
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Player.ChangeWeapon();
        }
#endif
    }
}