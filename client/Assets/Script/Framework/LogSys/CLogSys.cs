#if UNITY_EDITOR
#define INNER_VER
#endif
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
/// <summary>
///V — Verbose  系统正常运行时的日志
///D — Debug    用来跟进Bug的日志
///W — Warning  需要注意的信息，但并不一定导致错误。
///E — Error    错误已经产生
///F — Fatal    严重错误已经产生
/// </summary>
[SerializeField]
public enum ELogLevel
{
    Verbose,
    Debug,
    Warning,
    Error,
    Fatal,
}

/// <summary>
/// 专门为某一个系统或流程定义一个tag，以便筛选
/// </summary>
public enum ELogTag
{
    None,
    LogSys,
    GameRoot,
    Event,
    ResourceSys,
    PreLoadSys,
    Login,
    NetSys,
    UnityLog,
}

/// <summary>
/// 日志系统
/// 
/// 记录本地
///     路径：IOS和Android下是 Application.persistentDataPath + "/Log"。Editor下是Unity项目下的Log目录
///     一定时间后删除，暂定5天。
/// 
/// 日志收集
///     在“日志收集配置表.xlsx”中配置了关注的上报内容。
///     可针对某一tag和level的日志进行一定几率的采样收集。
///     也可以对特定openid或uid收集五天内特定时间中的日志。
///     
/// 已经记录的日志包括：
///     状态切换日志 ok
///     事件派发异常 ok
///     BUG跟进日志 ok
///     下载 ok
///     中断重连 ok
///     表格加载 ok
///     系统信息、硬件信息
///     登陆流程
///     帧频采样/网速采样
///  
/// 崩溃回调处理： 
///     IOS：Object C 回调  exp_call_back_func setAttachLog ok
///     Android：Java 回调  CrashHandleListener ok
///     
///     设置额外信息 最近的日志信息   10K 以内   ios ok, 
///     日志flush。ok
///     
/// 提交时间：
///     从Login界面登陆后提交 ok
///     长时间的中断返回，直接回到login界面进loading界面？ todo
/// 
/// 如何避免同样的错误信息刷屏？ ok
/// </summary>
public class CLogSys : CGameSystem
{
    private const string cLogPrefix = "[VLOG]";

    private static string mLogFilePath;
    private static string mLogDirPath;
    private static ILogPlatform mLogPlatform;
#if INNER_VER
    private ELogLevel mSelfLogLevel = ELogLevel.Error;
#else
    private ELogLevel mSelfLogLevel = ELogLevel.Error;
#endif

    public static CLogSys Instance
    {
        get;
        set;
    }
    public override void RegEvent()
    {
        EventDispatcher.AddListener(EventNames.GamePause, OnGamePause);
        EventDispatcher.AddListener(EventNames.AwakeFromPauseStart, OnAwakeFromPauseStart);
    }

    private void OnAwakeFromPauseStart()
    {
        Log(ELogLevel.Verbose, ELogTag.LogSys, "AwakeFromPause");
    }


    private void OnGamePause()
    {
        Log(ELogLevel.Verbose, ELogTag.LogSys, "GamePause");
        mLogPlatform.Flush();
    }

    public override void SysInitial()
    {
        base.SysInitial();
        Instance = this;

        mLogPlatform = new CLogPlatformEditor();

        System.DateTime now = System.DateTime.Now;
        mLogDirPath = "Log";
        mLogFilePath = string.Format("{6}/{0:00}{1:00}{2:00}{3:00}{4:00}{5:00}.log",
            now.Year - 2000, now.Month, now.Day, now.Hour, now.Minute, now.Second, mLogDirPath);

        //DeleteOldLog();
        OpenLogFile();

        Log(ELogLevel.Verbose, ELogTag.LogSys, string.Format("Start Log {0}", mLogFilePath));
    }

    private int mDeleteDays = 5;

    private void DeleteOldLog()
    {
        System.DateTime now = System.DateTime.Now;
        if (!Directory.Exists(mLogDirPath))
            Directory.CreateDirectory(mLogDirPath);
        try
        {
            foreach (string filePath in Directory.GetFiles(mLogDirPath))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                bool delete = false;
                int year, month, day;


                if (!int.TryParse(fileName.Substring(0, 2), out year))
                    delete = true;
                else if (!int.TryParse(fileName.Substring(2, 2), out month))
                    delete = true;
                else if (!int.TryParse(fileName.Substring(4, 2), out day))
                    delete = true;
                else
                {
                    System.DateTime logTime = new System.DateTime(year + 2000, month, day);
                    if ((now - logTime).Days > mDeleteDays)
                        delete = true;
                }
                if (delete)
                    File.Delete(filePath);

            }
        }
        catch (System.Exception)
        {
        }
    }

    public override void SysFinalize()
    {
        base.SysFinalize();

        Log(ELogLevel.Verbose, ELogTag.LogSys, string.Format("End Log {0}", mLogFilePath));

        CloseLogFile();
    }

    private static void OpenLogFile()
    {
#if !UNITY_EDITOR
        mLogPlatform.OpenLogFile(mLogFilePath);
#endif
    }

    private static void CloseLogFile()
    {
#if !UNITY_EDITOR
        mLogPlatform.CloseLogFile();
#endif
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.StartsWith(cLogPrefix)) return;

        ELogLevel level = ELogLevel.Verbose;
        switch (type)
        {
            case LogType.Log:
                level = ELogLevel.Verbose;
                break;
            case LogType.Warning:
                level = ELogLevel.Warning;
                break;
            case LogType.Error:
                level = ELogLevel.Error;
                break;
            case LogType.Exception:
                level = ELogLevel.Error;
                break;
            case LogType.Assert:
                level = ELogLevel.Warning;
                break;
        }

        Log(level, ELogTag.UnityLog, logString, stackTrace);
    }

    public static void Log(string content)
    {
        Log(ELogLevel.Debug, ELogTag.LogSys, content, null);
    }

    public static void Log(ELogLevel level, ELogTag tag, string content)
    {
        Log(level, tag, content, null);
    }

    public static void Error(string content)
    {
        Log(ELogLevel.Error, ELogTag.LogSys, content, null);
    }

    private static string mSameLogStr;
    private static int mSameLogCnt;

    public static void Log(ELogLevel level, ELogTag tag, string content, string stack)
    {
        CLogSys logSys = CGameRoot.GetGameSystem<CLogSys>();
        if (logSys == null) return;
        if ((int)level < (int)logSys.mSelfLogLevel)
            return;

        //if (FightScene.mInstance != null && FightScene.mInstance.isBattleStart)
        //{
        //    return;
        //}

        #region 重复内容保护
        string sameStr = content.Substring(0, Mathf.Min(20, content.Length));
        if (string.IsNullOrEmpty(mSameLogStr) || mSameLogStr != sameStr)
        {
            mSameLogStr = sameStr;
            mSameLogCnt = 0;
        }
        else
        {
            mSameLogCnt++;
        }
        if (mSameLogCnt > 20)
            return;
        #endregion

#if !UNITY_EDITOR
        if (string.IsNullOrEmpty(stack))
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < stackFrames.Length; ++i)
            {
                System.Reflection.MethodBase method = stackFrames[i].GetMethod();
                string typeName = method.DeclaringType.FullName;
                string methodName = method.Name;
                if (typeName == "CLogSys"
                    || typeName == "UnityEngine.Debug"
                    || methodName == "CallLogCallback") continue;

                sb.AppendFormat("{0}:{1}\n", typeName, methodName);
            }
            stack = sb.ToString();
        }

        char[] contentInLine;
        int contentIdx;
        TransInLine(content, out contentInLine, out contentIdx);

        char[] stackInLine;
        int stackIdx;
        TransInLine(stack, out stackInLine, out stackIdx);

        System.DateTime now = System.DateTime.Now;
        string curTime = string.Format("{0:00}{1:00}{2:00}{3:00}{4:00}", now.Month, now.Day, now.Hour, now.Minute, now.Second);
        string logStr = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", cLogPrefix, curTime, level.ToString()[0], tag,
            new string(contentInLine, 0, contentIdx), new string(stackInLine, 0, stackIdx));

        //ELogLevel logLevel = (ResourceSys != null && ResourceSys.RootCfg != null) ? ResourceSys.RootCfg.mLogLevel : ELogLevel.Debug;
        if (mLogPlatform != null)
        {
            mLogPlatform.Log(logStr);
        }

        if (level >= ELogLevel.Error)
        {
            //DCProxy.ReportError(content, logStr);
        }

#else
        if (level == ELogLevel.Warning)
            Debug.LogWarning(content);
        else if (level == ELogLevel.Error)
            Debug.LogError(content);
        else if (level == ELogLevel.Fatal)
            Debug.LogError(content);
        else if (level == ELogLevel.Debug)
            Debug.LogWarning(content);
        else if (level == ELogLevel.Verbose)
            Debug.Log(content);
#endif
    }

    private static void TransInLine(string contentWithStack, out char[] contentInLine, out int idx)
    {
        contentInLine = new char[contentWithStack.Length * 2 + 1];
        idx = 0;
        for (int i = 0; i < contentWithStack.Length; ++i)
        {
            if (contentWithStack[i] == '\n')
            {
                contentInLine[idx] = '\\';
                contentInLine[idx + 1] = 'n';
                idx += 2;
            }
            else if (contentWithStack[i] == '\r') { }
            else
            {
                contentInLine[idx] = contentWithStack[i];
                idx++;
            }
        }
    }

}

public class CLogSubmitFile
{
    public List<CLogSubmitData> mList;

    public static void Serialize(Stream stream, CLogSubmitFile data)
    {
        XmlSerializer xs = new XmlSerializer(typeof(CLogSubmitFile));
        TextWriter writer = new StreamWriter(stream, Encoding.UTF8);
        xs.Serialize(writer, data);
    }

    public static CLogSubmitFile Deserialize(Stream stream)
    {
        XmlSerializer xs = new XmlSerializer(typeof(CLogSubmitFile));
        return (CLogSubmitFile)xs.Deserialize(stream);
    }
}

public class CLogSubmitData
{
    public int mId;
    public byte mVer;
}
