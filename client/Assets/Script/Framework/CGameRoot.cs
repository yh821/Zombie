#define DEBUG_LOG
//#define PROFILE

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public delegate void DGameStateChangeEventHandler(EStateType newState, EStateType oldState);

public interface IGameSys
{
}
public class CGameRoot : MonoBehaviour
{
    public const string cRootName = "_GameRoot";
    static public event DGameStateChangeEventHandler OnStateChange;
    static public event DGameStateChangeEventHandler OnPreStateChange;
    static public event DGameStateChangeEventHandler OnPostStateChange;

    public bool mShowDebugLog = true;
    public Font[] usedFonts;

    static private CGameRoot mInstance;
    static public CGameRoot Instance
    {
        get
        {
            return mInstance;
        }
    }

    static public bool IsInitialed
    {
        get
        {
            if (Instance == null) return false;
            else return Instance.mIsInitialed;
        }
    }

    static public void SwitchToState(EStateType stateType)
    {
        if (stateType == EStateType.None) return;

        //if (stateType == EStateType.FightSelectMode)
        //{
        //    Debug.LogError("不是错误，打的日志: Enter FightSelectMode");
        //}

        Instance._SwitchToState(stateType);
    }

    static public T GetGameSystem<T>()
        where T : CGameSystem
    {
        if (Instance == null) return null;
        else return Instance._GetGameSystem<T>();
    }

    static public CGameSystem GetGameSystem(Type type)
    {
        if (Instance == null) return null;
        else return Instance._GetGameSystem(type);
    }

    static public bool HaveSystemRegisted(Type type)
    {
        return Instance._HaveSystemRegisted(type);
    }

    static public EStateType PreState
    {
        get
        {
            if (Instance != null)
                return Instance.mPreState;
            else
                return EStateType.None;
        }
    }

    static public EStateType CurState
    {
        get
        {
            if (Instance != null)
                return Instance.mOldState;
            else
                return EStateType.None;
        }
    }

    static public EGameRootCfgType ConfigType
    {
        get
        {
            if (Instance != null)
                return Instance.mConfigType;
            else
                return EGameRootCfgType.EditorBase;
        }
    }

    public EGameRootCfgType mConfigType = EGameRootCfgType.Game;
    public EStateType mFirstStateName = EStateType.None;

    private CGameRootCfg mConfig;
    private EStateType mOldState = EStateType.None;
    private EStateType mPreState = EStateType.Root;

    private bool mIsInitialed = false;


    private CGameSystem[] mSystems;
    private Dictionary<Type, CGameSystem> mSystemMap = new Dictionary<Type, CGameSystem>();


    public void Awake()
    {
        mInstance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(UINodesManager.UIRoot);
        CGameRootCfg.mCfgs[0] = null;
        CGameRootCfg.mCfgs[(int)EGameRootCfgType.Game] = CGameRootCfg.mGame;
        CGameRootCfg.mCfgs[(int)EGameRootCfgType.EditorBase] = CGameRootCfg.mEditorBase;
        mConfig = CGameRootCfg.mCfgs[(int)mConfigType];
    }

    public void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if !UNITY_EDITOR
        mShowDebugLog = false;
#endif

#if !UNITY_EDITOR
        //Application.targetFrameRate = 30;
#endif

        QualitySettings.vSyncCount = 0;

        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        //intial all system
        mSystems = mConfig.mInitialDelegate(transform);
        foreach (CGameSystem gameSys in mSystems)
        {
            mSystemMap.Add(gameSys.GetType(), gameSys);
        }
        foreach (CGameSystem gameSys in mSystems)
        {
            DateTime dtStart = DateTime.Now;
            gameSys.SysInitial();
        }

        //switch to first state
        if (mFirstStateName != EStateType.None)
        {
            SwitchToState(mFirstStateName);
        }
        else
        {
            _SwitchToState(EStateType.Root);
        }

        mIsInitialed = true;
        //setDesignContentScale();
    }

    private void OnDestroy()
    {
        foreach (CGameSystem gameSys in mSystems)
        {
            gameSys.SysFinalize();
        }
    }

    private int m_nFPS;
    public int realFPS
    {
        get { return m_nFPS; }
    }
    private int m_frameCount;
    private float m_fLastFpsUpdateTime;
    public void UpdateFPS()
    {
        m_frameCount++;
        if (Time.realtimeSinceStartup - m_fLastFpsUpdateTime > 1.0f)
        {
            m_nFPS = Mathf.CeilToInt(m_frameCount / (Time.realtimeSinceStartup - m_fLastFpsUpdateTime));
            m_frameCount = 0;
            m_fLastFpsUpdateTime = Time.realtimeSinceStartup;
        }
    }

    //private float m_fAccumilatedTime;
    //public static float m_fLogicFrameLength = 0.05f; //逻辑帧设定为20帧/s
    //private int m_nCurFrame;

    public void Update()
    {
        if (mSystems == null) return;
        CGameSystem gameSystem;
        for (int i = 0; i < mSystems.Length; ++i)
        {
            gameSystem = mSystems[i];
            if (gameSystem.SysEnabled)
                gameSystem.SysUpdate();
        }

        //m_fAccumilatedTime += Time.unscaledDeltaTime;
        //if (m_fAccumilatedTime > m_fLogicFrameLength)
        //{
        //    while (m_fAccumilatedTime > m_fLogicFrameLength)
        //    {
        //        m_nCurFrame++;
        //        m_fAccumilatedTime -= m_fLogicFrameLength;
        //    }

        //    OnLogicFrameTurn();
        //}

        UpdateFPS();
    }

    //public void OnLogicFrameTurn()
    //{
    //    CGameSystem gameSystem;
    //    for (int i = 0; i < mSystems.Length; ++i)
    //    {
    //        gameSystem = mSystems[i];
    //        if (gameSystem.SysEnabled)
    //            gameSystem.SysLogicFrameTurn();
    //    }

    //    //Debug.Log("Frame:" + m_nCurFrame.ToString());
    //}
    #region 中断返回

    public bool IsPause
    {
        get { return mIsPause; }
    }

    private bool mIsPause = false;
    //void OnApplicationPause(bool paused)
    //{
    //    if (paused)
    //    {
    //    }
    //    else
    //    {
    //        setDesignContentScale();
    //    }
    //}

    public void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            EventDispatcher.Broadcast(EventNames.GamePause);
        }
        else
        {
            EventDispatcher.Broadcast(EventNames.AwakeFromPauseStart);
        }
    }

    #endregion

    #region Switch To State

    private Queue<EStateType> switchQueue = new Queue<EStateType>();

    private void _SwitchToState(EStateType newState)
    {
        switchQueue.Enqueue(newState);
        //if (newState == EStateType.FightSelectMode)
        //{
        //    Debug.LogError(newState);
        //}
        if (runing == false)
            StartCoroutine(HandleSwitchQueue());
    }

    bool runing = false;

    private IEnumerator HandleSwitchQueue()
    {
        runing = true;
        while (switchQueue.Count != 0)
        {
            yield return StartCoroutine(_SwitchToStateCo(switchQueue.Dequeue()));
        }
        runing = false;
    }

    private static EStateType mWillStateType = EStateType.None;
    public static EStateType willStateType
    {
        get { return mWillStateType; }
    }

    private IEnumerator _SwitchToStateCo(EStateType newState)
    {
        mWillStateType = newState;
        if (mOldState == newState)
        {
            //Debug.LogWarning("SwitchState oldState == newState: " + newState);
            yield break;
        }

        CUility.Log("Begin: SwitchToStateCo " + newState);

        try
        {
            if (OnPreStateChange != null)
                OnPreStateChange(newState, mOldState);
        }
        catch (Exception ex)
        {
            CUility.LogError("Exception in OnPreStateChange: {0}", ex.ToString());
        }

        CGameState[] oldStates = mConfig.mStateMap[mOldState];
        CGameState[] newStates = mConfig.mStateMap[newState];

        int sameDepth = -1;
        while (sameDepth + 1 < newStates.Length && sameDepth + 1 < oldStates.Length
            && newStates[sameDepth + 1] == oldStates[sameDepth + 1])
        {
            ++sameDepth;
        }
        //Debug.Log(sameDepth);

        //Debug.Log("Leave old system");
        List<CGameSystem> leaveSystems = new List<CGameSystem>();
        for (int i = oldStates.Length - 1; i > sameDepth; --i)
        {
            foreach (Type sysType in oldStates[i].mSystems)
            {
                leaveSystems.Add(mSystemMap[sysType]);
            }
        }

        foreach (CGameSystem leaveSystem in leaveSystems)
        {
            CUility.Log("Leave System {0}", leaveSystem.Name);
            try
            {
                leaveSystem._SysLeave();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Exception in {1}'s SysLeave: {0}", ex.ToString(), leaveSystem.Name));
            }
        }

        CUility.Log("Leave System Finish");

        try
        {
            if (OnStateChange != null)
                OnStateChange(newState, mOldState);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Exception in OnStateChange: {0}", ex.ToString()));
        }

        CUility.Log("Start EnterSystem");

        //Debug.Log("Enter new system");
        List<CGameSystem> enterSystems = new List<CGameSystem>();
        for (int i = sameDepth + 1; i < newStates.Length; ++i)
        {
            foreach (Type sysType in newStates[i].mSystems)
            {
                if (!mSystemMap.ContainsKey(sysType))
                    throw new Exception(string.Format("SystemMap.ContainsKey({0}) == false", sysType.Name));

                mSystemMap[sysType].EnableInState = newStates[i].mStateType;
                enterSystems.Add(mSystemMap[sysType]);
            }
        }


        for (int i = 0; i < enterSystems.Count; ++i)
        //foreach (CGameSystem enterSystem in enterSystems)
        {
            CGameSystem enterSystem = enterSystems[i];

            CUility.Log("{0} SysEnter Start", enterSystem.Name);

            bool haveEnterCo = false;
            try
            {
                haveEnterCo = enterSystem._SysEnter();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Exception in {1}'s SysEnter: {0}", ex.ToString(), enterSystem.Name));
            }

            if (haveEnterCo)
            {
                yield return StartCoroutine(enterSystem.SysEnterCo());
            }

            enterSystem._EnterFinish();

            //CLogSys.Log(ELogLevel.Verbose, ELogTag.GameRoot, string.Format("{0} SysEnter Finish", enterSystem.Name));
        }

        //Debug.Log("Add new system...............Finish");


        //Debug.Log("Remove old system once more");

        //加入了新系统之后，再给旧系统一次清理的机会。
        foreach (CGameSystem leaveSystem in leaveSystems)
        {
            leaveSystem.SysLastLeave();
        }

        //Debug.Log("Remove old system once more...............Finish");

        foreach (CGameSystem enterSystem in enterSystems)
        {
            try
            {
                enterSystem.OnStateChangeFinish();
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in OnStateChangeFinish: +" + ex.ToString());
            }
        }

        //Debug.Log(string.Format("Enter State {0} from {1}", newState, mOldState));

        mPreState = mOldState;
        mOldState = newState;

        try
        {
            if (OnPostStateChange != null)
                OnPostStateChange(newState, mPreState);
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("Exception in OnPostStateChange: " + ex.ToString()));
        }

        CUility.Log("End: SwitchToStateCo " + newState);
    }

    #endregion



    private T _GetGameSystem<T>()
        where T : CGameSystem
    {
        if (mSystemMap.ContainsKey(typeof(T)))
            return (T)mSystemMap[typeof(T)];
        else
            return null;
    }

    private CGameSystem _GetGameSystem(Type type)
    {
        if (mSystemMap.ContainsKey(type))
            return mSystemMap[type];
        else
            return null;
    }

    private bool _HaveSystemRegisted(Type type)
    {
        return mSystemMap.ContainsKey(type);
    }
}