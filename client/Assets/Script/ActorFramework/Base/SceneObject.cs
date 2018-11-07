using Base;
using UnityEngine;

public abstract partial class SceneObject : MonoBehaviour, ISceneObject
{
    #region 变量
    /// <summary>
    /// 唯一ID
    /// </summary>
    public long ID { get; protected set; }

    /// <summary>
    /// 是否进场
    /// </summary>
    public bool IsEnterScene { get; protected set; }
    #endregion

    #region 内部调用
    private void Update()
    {
        if (IsEnterScene)
            OnUpdate();
    }

    private void Awake()
    {
        OnCreate();
    }

    private void OnDestroy()
    {
        OnDelete();
    }
    #endregion

    #region 接口
    public virtual void OnCreate()
    {
        IsEnterScene = false;
    }

    public virtual void EnterScene()
    {
        ID = IdGenerater.GenerateId();
        IsEnterScene = true;
    }

    public virtual void LeaveScene()
    {
        if (IsEnterScene)
        {
            IsEnterScene = false;
            gameObject.SetActive(false);
            ObjectPoolSys.Instance.Destroy(gameObject);
        }
    }

    public virtual void OnDelete()
    {
    }

    public virtual void OnUpdate()
    {
    }
    #endregion
}
