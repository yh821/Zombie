using UnityEngine;

public class EffectManager : CGameSystem
{
    public static Transform EffectRoot
    {
        get
        {
            if (null == _effectRoot)
            {
                GameObject objRoot = new GameObject();
                objRoot.name = "EffectRoot";

                _effectRoot = objRoot.transform;
                _effectRoot.position = Vector3.zero;
                _effectRoot.localScale = Vector3.one;
            }

            return _effectRoot.transform;
        }
    }
    private static Transform _effectRoot;

    public static GameObject Create(string strName, bool bIsAutoDestory, Transform transParent = null)
    {
        Object objPrefab = CResourceSys.Instance.LoadEffect(strName);
        GameObject objEffect = ObjectPoolSys.Instance.Create(objPrefab);
        
        EffectObject objectEffect = objEffect.GetComponent<EffectObject>();
        if (null == objectEffect)
        {
            objectEffect = objEffect.AddComponent<EffectObject>();
        }

        //有些拖尾特效需要缓存一段时间才能用
        if (!objectEffect.CanActive())
        {
            ObjectPoolSys.Instance.Destroy(objEffect);

            objEffect = ObjectPoolSys.Instance.Create(objPrefab, true);
            objectEffect = objEffect.AddComponent<EffectObject>();
        }

        if (null == transParent)
        {
            objEffect.transform.parent = EffectRoot;
        }
        else
        {
            objEffect.transform.parent = transParent;
        }

        objEffect.transform.localEulerAngles = Vector3.zero;
        objEffect.transform.localPosition = Vector3.zero;

        objectEffect.Begin(bIsAutoDestory);

        return objEffect;
    }

    public static void Destory(GameObject objEffect)
    {
        if (null == objEffect)
        {
            return;
        }

        EffectObject objectEffect = objEffect.GetComponent<EffectObject>();
        if (null != objectEffect)
        {
            objectEffect.End();
        }

        objEffect.transform.parent = EffectRoot;
        ObjectPoolSys.Instance.Destroy(objEffect);
    }
}
