using System;
using System.Diagnostics;
using UnityEngine.Profiling;

/// <summary>
/// 定时触发器
/// </summary>
public class CTimeSys : CGameSystem
{
    #region 变量

    public static CTimeSys Instance { get; private set; }

    private uint m_nNextTimerId;
    private uint m_unTick;
    private KeyedPriorityQueue<uint, AbsTimerData, ulong> m_queue;
    private Stopwatch m_stopWatch;
    private readonly object m_queueLock = new object();


    private float m_deltaSystemTimer = 1000f;
    private float m_timeSystemTimer;
    private float m_timeSystemTimerTmp;
    private float m_checkPerTime = 10.0f;
    private float m_checkTimeTmp;
    private float m_invokeReaptingTime = 0.02f;

    private Action m_cheatHandler;
    private bool m_cheat;

    #endregion

    #region override

    public override void SysInitial()
    {
        m_cheat = false;
        m_queue = new KeyedPriorityQueue<uint, AbsTimerData, ulong>();
        m_stopWatch = new Stopwatch();

        System.Timers.Timer t = new System.Timers.Timer(m_deltaSystemTimer);
        t.Elapsed += Theout;//到达时间的时候执行事件； 
        t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)； 
        t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

        Instance = this;
        InvokeRepeating("Tick", 0f, 0.03f);

        base.SysInitial();
    }

    public override void SysFinalize()
    {
        CancelInvoke("Tick");
        m_queue.Clear();
        base.SysFinalize();
    }

    #endregion

    #region 逻辑

    public void Theout(object source, System.Timers.ElapsedEventArgs e)
    {
        m_timeSystemTimer += m_deltaSystemTimer / 1000f;
        m_timeSystemTimerTmp = m_timeSystemTimer;
    }

    #region AddTimer

    /// <summary>
    /// 添加定时对象
    /// </summary>
    /// <param name="start">延迟启动时间。（毫秒）</param>
    /// <param name="interval">重复间隔，为零不重复。（毫秒）</param>
    /// <param name="handler">定时处理方法</param>
    /// <returns>定时对象Id</returns>
    public uint AddTimer(uint start, int interval, Action handler)
    {
        //起始时间会有一个tick的误差,tick精度越高,误差越低
        var p = GetTimerData(new TimeData(), start, interval);
        p.Action = handler;
        return AddTimer(p);
    }

    /// <summary>
    /// 添加定时对象
    /// </summary>
    /// <typeparam name="T">参数类型1</typeparam>
    /// <param name="start">延迟启动时间。（毫秒）</param>
    /// <param name="interval">重复间隔，为零不重复。（毫秒）</param>
    /// <param name="handler">定时处理方法</param>
    /// <param name="arg1">参数1</param>
    /// <returns>定时对象Id</returns>
    public uint AddTimer<T>(uint start, int interval, Action<T> handler, T arg1)
    {
        var p = GetTimerData(new TimerData<T>(), start, interval);
        p.Action = handler;
        p.Arg1 = arg1;
        return AddTimer(p);
    }

    /// <summary>
    /// 添加定时对象
    /// </summary>
    /// <typeparam name="T">参数类型1</typeparam>
    /// <typeparam name="U">参数类型2</typeparam>
    /// <param name="start">延迟启动时间。（毫秒）</param>
    /// <param name="interval">重复间隔，为零不重复。（毫秒）</param>
    /// <param name="handler">定时处理方法</param>
    /// <param name="arg1">参数1</param>
    /// <param name="arg2">参数2</param>
    /// <returns>定时对象Id</returns>
    public uint AddTimer<T, U>(uint start, int interval, Action<T, U> handler, T arg1, U arg2)
    {
        var p = GetTimerData(new TimerData<T, U>(), start, interval);
        p.Action = handler;
        p.Arg1 = arg1;
        p.Arg2 = arg2;
        return AddTimer(p);
    }

    /// <summary>
    /// 添加定时对象
    /// </summary>
    /// <typeparam name="T">参数类型1</typeparam>
    /// <typeparam name="U">参数类型2</typeparam>
    /// <typeparam name="V">参数类型3</typeparam>
    /// <param name="start">延迟启动时间。（毫秒）</param>
    /// <param name="interval">重复间隔，为零不重复。（毫秒）</param>
    /// <param name="handler">定时处理方法</param>
    /// <param name="arg1">参数1</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <returns>定时对象Id</returns>
    public uint AddTimer<T, U, V>(uint start, int interval, Action<T, U, V> handler, T arg1, U arg2, V arg3)
    {
        var p = GetTimerData(new TimerData<T, U, V>(), start, interval);
        p.Action = handler;
        p.Arg1 = arg1;
        p.Arg2 = arg2;
        p.Arg3 = arg3;
        return AddTimer(p);
    }

    #endregion

    #region DelTimer

    /// <summary>
    /// 删除定时对象
    /// </summary>
    /// <param name="timerId">定时对象Id</param>
    public void DelTimer(uint timerId)
    {
        lock (m_queueLock)
            m_queue.Remove(timerId);
    }

    #endregion

    /// <summary>
    /// 立即执行某个timer的回调
    /// </summary>
    /// <param name="timerId"></param>
    public void ExecuteImmediately(uint timerId)
    {
        m_queue.Get(timerId).DoAction();
        DelTimer(timerId);
    }

    public void AddCheatCheckHandler(Action handler)
    {
        m_cheatHandler = handler;
    }

    /// <summary>
    /// 检测是否用了加速器
    /// </summary>
    private void CheckCheat()
    {
        if (m_timeSystemTimerTmp - m_timeSystemTimer > 5.0f)
        {
            m_cheat = true;
            m_cheatHandler();
        }
    }

    /// <summary>
    /// 定时器暂停功能
    /// </summary>
    private static bool isPause = false;
    public static void Pause(bool _isPause)
    {
        isPause = _isPause;
    }

    /// <summary>
    /// 周期调用触发任务
    /// </summary>
    public void Tick()
    {
        if (isPause) return;
        if (!m_stopWatch.IsRunning)
            m_stopWatch.Start();

        m_unTick = (uint)(UnityEngine.Time.time * 1000);

        m_checkTimeTmp += m_invokeReaptingTime;
        m_timeSystemTimerTmp += m_invokeReaptingTime;
        if (m_cheat == false)
        {
            if (m_checkTimeTmp > m_checkPerTime)
            {
                m_checkTimeTmp = 0;
                CheckCheat();
            }
        }

        bool profilerSample = UnityEngine.Debug.isDebugBuild || UnityEngine.Application.isEditor;
        while (m_queue.Count != 0)
        {
            AbsTimerData p;
            lock (m_queueLock)
                p = m_queue.Peek();
            if (m_unTick < p.UnNextTick)
            {
                break;
            }
            lock (m_queueLock)
                m_queue.Dequeue();
            if (p.NInterval > 0)
            {
                p.UnNextTick += (ulong)p.NInterval;
                lock (m_queueLock)
                    m_queue.Enqueue(p.NTimerId, p, p.UnNextTick);
                if (profilerSample)
                {
                    var name = string.IsNullOrEmpty(p.StackTrack) ? p.Action.Method.Name : p.StackTrack;
                    Profiler.BeginSample(name);
                }
                p.DoAction();
                if (profilerSample)
                {
                    Profiler.EndSample();
                }
            }
            else
            {
                if (profilerSample)
                {
                    var name = string.IsNullOrEmpty(p.StackTrack) ? p.Action.Method.Name : p.StackTrack;
                    Profiler.BeginSample(name);
                }
                p.DoAction();
                if (profilerSample)
                {
                    Profiler.EndSample();
                }
            }
        }
    }

    /// <summary>
    /// 重置定时触发器
    /// </summary>
    public void Reset()
    {
        m_unTick = 0;
        m_nNextTimerId = 0;
        lock (m_queueLock)
            while (m_queue.Count != 0)
                m_queue.Dequeue();
    }

    private uint AddTimer(AbsTimerData p)
    {
        if (UnityEngine.Debug.isDebugBuild)
        {
            var frame = new StackFrame(2, true);
            var fileName = UnityEngine.Application.isMobilePlatform ? frame.GetFileName().Replace('\\', '/') : frame.GetFileName();
            p.StackTrack = string.Format("[{0}, {1}]", System.IO.Path.GetFileName(fileName), frame.GetFileLineNumber());
        }

        lock (m_queueLock)
            m_queue.Enqueue(p.NTimerId, p, p.UnNextTick);
        return p.NTimerId;
    }

    private T GetTimerData<T>(T p, uint start, int interval) where T : AbsTimerData
    {
        p.NInterval = interval;
        p.NTimerId = ++m_nNextTimerId;
        p.UnNextTick = m_unTick + 1 + start;
        return p;
    }

    #endregion

    #region 时间格式
    public enum TimeFormat
    {
        HHMMSS,
        MMSS,
        HHMM,
    }

    /// <summary>
    /// 时间格式
    /// </summary>
    public static string TimeString(int time, TimeFormat format = TimeFormat.MMSS)
    {
        int hour = time / 3600;
        int minute = time % 3600 / 60;
        int second = time % 60;
        switch (format)
        {
            case TimeFormat.HHMMSS:
                return string.Format("{0}:{1}:{2}", hour.ToString("D2"), minute.ToString("D2"), second.ToString("D2"));
            case TimeFormat.HHMM:
                return string.Format("{0}:{1}", hour.ToString("D2"), minute.ToString("D2"));
            case TimeFormat.MMSS:
                return string.Format("{0}:{1}", minute.ToString("D2"), second.ToString("D2"));
            default:
                return string.Empty;
        }
    }
    #endregion
}