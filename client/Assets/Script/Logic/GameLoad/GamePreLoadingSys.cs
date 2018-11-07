using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//处理在登陆选择角色之后加载一些常用资源如：表格
public class GamePreLoadingSys : CGameSystem 
{
    private const string m_cLoadingUI = "UI/LoadingUI/LoadingUI";
    private bool m_isFirstLoad = true;

    private static GamePreLoadingSys m_gamePreLoadingSys;
    public static GamePreLoadingSys Instance { get { return m_gamePreLoadingSys; } }

    private EStateType m_nextStateType = EStateType.GameLoading;
    public override void SysInitial()
    {
        base.SysInitial();
        m_gamePreLoadingSys = this;
    }

    public override bool SysEnter()
    {
        StartCoroutine(PreLoadingCo());
        return false;
    }

    private void SetProgress(float fProgress)
    {
    }

    public IEnumerator PreLoadingCo()
    {
        SetProgress(0);

        float fPercent = 0;
        if (m_isFirstLoad)
        {
            m_isFirstLoad = false;
            int nLoadedCount = 0;
            fPercent = 0.95f;
            TableResMgr.Instance.PreLoading();
            //foreach (KeyValuePair<int, TableResMgr.TableRecordBase> kv in TableResMgr.Instance.m_tablePathDict)
            //{
            //    int nIndex = kv.Key;
            //    TableResMgr.Instance.LoadBinary(nIndex);
            //    nLoadedCount++;
            //    SetProgress((float)nLoadedCount / (float)TableResMgr.Instance.m_tablePathDict.Count * fPercent);
            //    if (nLoadedCount % 15 == 0)
            //    {
            //        yield return null;
            //    }
            //}
        }

        fPercent = 1.0f;
        SetProgress(fPercent);

        CGameRoot.SwitchToState(m_nextStateType);

        yield return null;        
    }
}
