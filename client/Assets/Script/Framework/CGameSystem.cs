using System.Collections;
using UnityEngine;

public class CGameSystem : MonoBehaviour, IGameSys
{
    private string mName = null;
    public string Name
    {
        get
        {
            if (mName == null)
                mName = GetType().Name;
            return mName;
        }
    }

    public EStateType EnableInState
    {
        get { return mEnableInState; }
        set { mEnableInState = value; }
    }
    private EStateType mEnableInState;

    //for register event 
    public virtual void RegEvent() { }

    #region Initial
    /// <summary>
    /// 初始化系统[游戏运行立即执行]
    /// </summary>
    public virtual void SysInitial() { RegEvent(); }
    #endregion

    #region Finalize
    /// <summary>
    /// 释放系统
    /// </summary>
    public virtual void SysFinalize() { }
    #endregion

    #region Enter

    public bool SysEnabled { get { return mSysEnabled; } }
    protected bool mSysEnabled = false;
    public bool SysEntering { get { return mSysEntering; } }
    protected bool mSysEntering = false;

    public bool _SysEnter()
    {
        mSysEntering = true;
        return SysEnter();
    }

    public virtual bool SysEnter()
    {
        return false;
    }

    public virtual IEnumerator SysEnterCo()
    {
        yield break;
    }

    public void _EnterFinish()
    {
        mSysEnabled = true;
        mSysEntering = false;
    }

    #endregion

    #region Leave

    public void _SysLeave()
    {
        mSysEnabled = false;
        EnableInState = EStateType.None;
        SysLeave();
    }

    public virtual void SysLeave()
    {
    }

    public virtual void SysLastLeave()
    {

    }

    #endregion

    public virtual void SysUpdate()
    {

    }

    public virtual void OnStateChangeFinish()
    {
    }

    ////逻辑帧更新
    //public virtual void SysLogicFrameTurn()
    //{

    //}

    #region HelpFun

    //public static void Log(object obj)
    //{
    //    CUtility.Log(obj);
    //}

    //public static void Log(object obj, ELogLevel level)
    //{
    //    CUtility.Log(obj, level);
    //}

    //public static bool Assert(bool arg)
    //{
    //    return CUtility.Assert(arg);
    //}

    //public static bool Assert(bool arg, string notify)
    //{
    //    return CUtility.Assert(arg, notify);
    //}

    public static GameObject[] GetSelectGameObj()
    {
#if UNITY_EDITOR
        System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; ++i)
        {
            string name = assemblies[i].FullName.Split(',')[0];
            if (name == "Assembly-CSharp-Editor")
            {
                System.Type mEditorModeType = assemblies[i].GetType("CDebugToolsWindow");
                GameObject[] ret = (GameObject[])mEditorModeType.InvokeMember("GetSelectGameObj", System.Reflection.BindingFlags.InvokeMethod
                    | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                    null, null, new object[] { });

                return ret;
            }
        }
        return null;
#else
        return null;
#endif
    }

    public static bool IsSelectGameObj(GameObject go)
    {
        GameObject[] select = CGameSystem.GetSelectGameObj();
        if (select != null)
        {
            for (int i = 0; i < select.Length; ++i)
            {
                if (select[i] == go)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
}

