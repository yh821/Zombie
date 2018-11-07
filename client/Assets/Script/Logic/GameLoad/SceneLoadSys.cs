using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoadSys : CGameSystem
{
    private string mLevelName = "game_2";
    private AsyncOperation m_cAsyncOpera = null;

    private bool m_bIsDone = false;
    public bool isDone
    {
        get { return m_bIsDone; }
    }

    private static SceneLoadSys m_cInstance;
    public static SceneLoadSys instance
    {
        get { return m_cInstance;}
    }

    public override void SysInitial()
    {
        m_cInstance = this;
        base.SysInitial();
    }

    public override bool SysEnter()
    {
        m_bIsDone = false;

        CResourceSys.Instance.UnLoadAllAsset();

        string strLevelName = mLevelName;
        if (SceneManager.GetActiveScene().name != strLevelName)
        {
            m_cAsyncOpera = SceneManager.LoadSceneAsync(strLevelName);
        }
        return true;
    }

    public override void SysUpdate()
    {
        base.SysUpdate();
        if (m_cAsyncOpera.isDone)
        {
            m_bIsDone = true;
        }
    }

    public override void SysLeave()
    {
        base.SysLeave();

        m_bIsDone = false;
        m_cAsyncOpera = null;
    }
}
