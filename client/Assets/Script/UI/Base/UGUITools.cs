using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

static public class UGUITools
{
    #region [OpenURL]

    static public WWW OpenURL(string url)
    {
#if UNITY_FLASH
            Debug.LogError("WWW is not yet implemented in Flash");
            return null;
#else
        WWW www = null;
        try
        {
            www = new WWW(url);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        return www;
#endif
    }

    static public WWW OpenURL(string url, WWWForm form)
    {
        if (form == null) return OpenURL(url);
#if UNITY_FLASH
            Debug.LogError("WWW is not yet implemented in Flash");
            return null;
#else
        WWW www = null;
        try
        {
            www = new WWW(url, form);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex != null ? ex.Message : "<null>");
        }
        return www;
#endif
    }

    #endregion

    #region [随机数]

    static public int RandomRange(int min, int max)
    {
        if (min == max) return min;
        return UnityEngine.Random.Range(min, max + 1);
    }

    #endregion   

    #region [Instantiate]添加子对象到父结点 

    /// <summary>
    /// 复制Prefab
    /// </summary>
    /// <param name="parent">父节点</param>
    /// <param name="asset">Prefab Asset</param>
    /// <param name="forceShow">是否强制active</param>
    /// <param name="name">New对象的名称</param>
    /// <returns></returns>
    static public GameObject AddChild(GameObject parent, GameObject asset, bool forceShow, string name)
    {
        GameObject go = AddChild(parent, asset, forceShow);
        if (go != null && !string.IsNullOrEmpty(name))
            go.name = name;

        return go;
    }

    /// <summary>
    /// 复制Prefab
    /// </summary>
    /// <param name="parent">父节点</param>
    /// <param name="asset">prefab asset</param>
    /// <param name="forceShow">是否强制active</param>
    /// <returns></returns>
    static public GameObject AddChild(GameObject parent, GameObject asset, bool forceShow)
    {
        if (asset == null)
        {
#if UNITY_EDITOR
            Debug.LogError("prefab is null");
#endif
            return null;
        }

        GameObject goInstantiate = GameObject.Instantiate(asset) as GameObject;
        //ResourceManager.SetInstantiateUIRef(goInstantiate, null);
        SetParent(parent, goInstantiate, forceShow);
        return goInstantiate;
    }

    static public void SetParent(GameObject parent, GameObject goInstantiate, bool forceShow, string name)
    {
        if (goInstantiate != null && !string.IsNullOrEmpty(name))
            goInstantiate.name = name;
        SetParent(parent, goInstantiate, forceShow);
    }

    static public void SetParent(GameObject parent, GameObject goInstantiate, bool forceShow)
    {
        if (goInstantiate != null && forceShow)
            goInstantiate.SetActive(true);

        if (goInstantiate != null && parent != null)
        {
            Transform t = goInstantiate.transform;
            t.SetParent(parent.transform, false);
            goInstantiate.layer = parent.layer;

            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        else if (parent == null)
        {
            Transform t = goInstantiate.transform;

            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }

    #endregion

    #region [Transform]重置

    #region Transform

    //重置Transform,并设置根节点
    //static public void ResetTransform(Transform go, Transform parent, bool worldPositionStays)
    //{
    //    go.SetParent(parent, worldPositionStays);
    //    if (go is RectTransform)
    //        ResetTransform(go as RectTransform);
    //    else
    //        ResetTransform(go);
    //}

    //重置Transform,并设置根节点
    static public void ResetTransform(Transform go, Transform parent)
    {
        go.SetParent(parent);
        if (go is RectTransform)
            ResetTransform(go as RectTransform);
        else
            ResetTransform(go);
    }

    //重置Transform
    static public void ResetTransform(Transform go)
    {
        go.localPosition = Vector3.zero;
        go.localRotation = Quaternion.identity;
        go.localScale = Vector3.one;
    }

    #endregion

    #region RectTransform

    //重置Transform,并设置根节点
    static public void ResetTransform(RectTransform go, Transform parent)
    {
        if (go != null)
        {
            go.SetParent(parent);
            ResetTransform(go);
        }
        else
        {
            Debug.LogError("go is null");
        }
    }

    //重置Transform
    static public void ResetTransform(RectTransform go)
    {
        if (go != null)
        {
            go.localPosition = Vector3.zero;
            go.anchoredPosition = Vector2.zero;
            go.localRotation = Quaternion.identity;
            go.localScale = Vector3.one;
        }
        else
        {
            Debug.LogError("go is null");
        }
    }

    #endregion

    #endregion

    #region [根节点][子节点]

    //获取GameObject的根节点
    static public GameObject GetRoot(GameObject go)
    {
        Transform t = go.transform;

        for (;;)
        {
            Transform parent = t.parent;
            if (parent == null) break;
            t = parent;
        }
        return t.gameObject;
    }

    //获取目标父节点
    static public T GetTargetParent<T>(GameObject childNode) where T : Component
    {
        Transform t = childNode.transform;
        T compoent = t.GetComponent<T>();
        if (compoent != null)
            return compoent;
        return t.GetComponentInParent<T>();
    }

    //判断是否是子节点
    static public bool IsChild(Transform parent, Transform child)
    {
        if (parent == null || child == null) return false;

        while (child != null)
        {
            if (child == parent) return true;
            child = child.parent;
        }
        return false;
    }

    //获取GameObject的层次路径名称
    static public string GetHierarchy(GameObject obj)
    {
        if (obj == null) return "";
        string path = obj.name;

        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "\\" + path;
        }
        return path;
    }

    #endregion

    #region [FindInParents]

    static public T FindInParents<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
#if UNITY_FLASH
		object comp = go.GetComponent<T>();
#else
        T comp = go.GetComponent<T>();
#endif
        if (comp == null)
        {
            Transform t = go.transform.parent;

            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }
        }
#if UNITY_FLASH
		return (T)comp;
#else
        return comp;
#endif

    }

    static public T FindInParents<T>(Transform trans) where T : Component
    {
        if (trans == null) return null;
#if UNITY_4_3
#if UNITY_FLASH
		object comp = trans.GetComponent<T>();
#else
		T comp = trans.GetComponent<T>();
#endif
		if (comp == null)
		{
			Transform t = trans.transform.parent;

			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}
#if UNITY_FLASH
		return (T)comp;
#else
		return comp;
#endif
#else
        return trans.GetComponentInParent<T>();
#endif
    }

    #endregion

    #region [Active][Deactivate]

    static void Activate(Transform t, bool compatibilityMode)
    {
        SetActiveSelf(t.gameObject, true);

        if (compatibilityMode)
        {
            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform child = t.GetChild(i);
                if (child.gameObject.activeSelf) return;
            }
            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform child = t.GetChild(i);
                Activate(child, true);
            }
        }
    }

    static void Deactivate(Transform t) { SetActiveSelf(t.gameObject, false); }

    static public void SetActiveChildren(GameObject go, bool state)
    {
        Transform t = go.transform;

        if (state)
        {
            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform child = t.GetChild(i);
                Activate(child, true);
            }
        }
        else
        {
            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform child = t.GetChild(i);
                Deactivate(child);
            }
        }
    }

    [System.Diagnostics.DebuggerHidden]
    [System.Diagnostics.DebuggerStepThrough]
    static public bool GetActive(Behaviour mb)
    {
        return mb && mb.enabled && mb.gameObject.activeInHierarchy;
    }

    [System.Diagnostics.DebuggerHidden]
    [System.Diagnostics.DebuggerStepThrough]
    static public bool GetActive(GameObject go)
    {
        return go && go.activeInHierarchy;
    }

    [System.Diagnostics.DebuggerHidden]
    [System.Diagnostics.DebuggerStepThrough]
    static public void SetActiveSelf(GameObject go, bool state)
    {
        go.SetActive(state);
    }

    #endregion

    #region [Round]

    //将值舍入到最接近的整数或指定的小数位数
    static public Vector3 Round(Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    #endregion

    #region[Func]

    static public string GetFuncName(object obj, string method)
    {
        if (obj == null) return "<null>";
        string type = obj.GetType().ToString();
        int period = type.LastIndexOf('/');
        if (period > 0) type = type.Substring(period + 1);
        return string.IsNullOrEmpty(method) ? type : type + "/" + method;
    }

    static public void Execute<T>(GameObject go, string funcName) where T : Component
    {
        T[] comps = go.GetComponents<T>();

        T comp;
        for (int index = 0; index < comps.Length; ++index)
        {
            comp = comps[index];
#if !UNITY_EDITOR && (UNITY_WEBPLAYER || UNITY_FLASH || UNITY_METRO || UNITY_WP8)
			comp.SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
#else
            MethodInfo method = comp.GetType().GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null) method.Invoke(comp, null);
#endif
        }
    }

    #endregion
}