using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolSys : CGameSystem
{
    private Dictionary<Object, List<GameObject>> m_dicCacheObj = new Dictionary<Object, List<GameObject>>();

    private static ObjectPoolSys m_cInstance;
    public static ObjectPoolSys Instance
    {
        get { return m_cInstance; }
    }

    public Transform PoolRoot
    {
        get
        {
            if (null == mPoolRoot)
            {
                GameObject objRoot = new GameObject();
                objRoot.name = "PoolRoot";

                mPoolRoot = objRoot.transform;
                mPoolRoot.position = Vector3.zero;
                mPoolRoot.localScale = Vector3.one;
            }
            return mPoolRoot;
        }
    }
    private Transform mPoolRoot;

    public override void SysInitial()
    {
        base.SysInitial();
        m_cInstance = this;
    }

    public override void SysLeave()
    {
        base.SysLeave();

        ReleasePool();
        m_dicCacheObj.Clear();
    }

    public GameObject Create(Object cPrefab, bool bNeedNew = false)
    {
        if (null == cPrefab)
        {
            return null;
        }
        List<GameObject> lstObj = null; ;
        m_dicCacheObj.TryGetValue(cPrefab, out lstObj);

        if (null == lstObj)
        {
            lstObj = new List<GameObject>();
            m_dicCacheObj.Add(cPrefab, lstObj);
        }

        GameObject cObj = null;
        if (lstObj.Count <= 0 || bNeedNew)
        {
            cObj = GameObject.Instantiate(cPrefab) as GameObject;
            ObjectPool cPoolObject = cObj.AddUniqueComponent<ObjectPool>();
            cPoolObject.objPrefab = cPrefab;
        }
        else
        {
            cObj = lstObj[0];
            cObj.SetActive(true);
            lstObj.RemoveAt(0);
        }
        return cObj;
    }

    public GameObject Cache(Object cPrefab)
    {
        List<GameObject> lstObj = null; ;
        m_dicCacheObj.TryGetValue(cPrefab, out lstObj);

        if (null == lstObj)
        {
            lstObj = new List<GameObject>();
            m_dicCacheObj.Add(cPrefab, lstObj);
        }

        GameObject cObj = GameObject.Instantiate(cPrefab) as GameObject;
        ObjectPool cPoolObject = cObj.AddUniqueComponent<ObjectPool>();
        cPoolObject.objPrefab = cPrefab;
        cObj.SetActive(false);
        lstObj.Add(cObj);
        return cObj;
    }

    public bool Destroy(GameObject cObj)
    {
        ObjectPool cPoolObject = cObj.GetComponent<ObjectPool>();
        if (null == cPoolObject)
        {
            return false;
        }

        List<GameObject> lstObj = null;
        m_dicCacheObj.TryGetValue(cPoolObject.objPrefab, out lstObj);

        if (null == lstObj)
        {
            lstObj = new List<GameObject>();
            m_dicCacheObj.Add(cPoolObject.objPrefab, lstObj);
        }

        lstObj.Add(cObj);
        cObj.SetActive(false);
        cObj.transform.parent = PoolRoot;

        return true;
    }

    public void ReleasePool()
    {
        foreach (KeyValuePair<Object, List<GameObject>> kv in m_dicCacheObj)
        {
            for (int i = kv.Value.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(kv.Value[i]);
                kv.Value.RemoveAt(i);
            }
        }
    }
}
