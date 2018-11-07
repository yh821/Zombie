using UnityEngine;
using UnityEngine.AI;

public partial class Actor
{
    #region 变量
    public bool useGravity = true;

    public CharacterController Controller { get; protected set; }
    public NavMeshAgent Agent { get; protected set; }

    public Vector3 MovingDirection { get; protected set; }

    public float MovingSpeed { get { return Attr.MoveSpeed; } }

    public Vector3 Velocity { get { return MovingSpeed * MovingDirection; } }

    public Vector3 position
    {
        get { return transform.position; }
        set
        {
            NavMeshPosition = FixNavMeshPosition(value);
            transform.position = NavMeshPosition;
        }
    }

    protected Vector3 NavMeshPosition;
    protected NavMeshPath mNavMeshPath;
    protected NavMeshHit mNavMeshHit;
    protected bool IsClearTargetPosition = true;
    protected int ToCorner;
    protected float NextPace, LeftLength;
    protected System.Action MoveEndCallBack = null;
    #endregion

    #region 移动相关
    protected virtual void MoveProcess()
    {
        if (IsDead)
            return;

        //todo 常规位移
        ControllerMove(CommonMove()/*, useGravity*/);

        //todo 击退位移

        //todo 侧滑位移
    }

    /// <summary>
    /// 委托给网格导航移动
    /// </summary>
    //protected void NavMeshMove(Vector3 pos, float radius = 0.5f)
    //{
    //    IsClearTargetPosition = false;

    //    NavMeshPosition = FixNavMeshPosition(pos);

    //    Agent.transform.position = FixNavMeshPosition(Agent.transform.position);

    //    Agent.radius = radius;
    //    Agent.speed = Attr.MoveSpeed;

    //    Agent.SetDestination(NavMeshPosition);
    //}

    protected void ControllerMove(Vector3 offset, bool isUseGravity = false)
    {
        if (offset == Vector3.zero)
        {
            SetBool("move", false);
            return;
        }

        SetBool("move", true);
        Controller.Move(offset * Time.deltaTime);

        if (isUseGravity)
        {
            Vector3 pos;
            if (XUtility.GetMoveLayerHeight(transform.position, out pos) && transform.position.y > pos.y)
            {
                Controller.Move(Physics.gravity * Time.deltaTime);
            }
        }
    }

    protected void ControllerNavMeshMove(Vector3 pos, float radius = 0.5f, System.Action callback = null)
    {
        IsClearTargetPosition = false;

        Agent.radius = radius;
        NavMeshPosition = FixNavMeshPosition(pos);
        NavMesh.CalculatePath(Agent.transform.position, NavMeshPosition, NavMesh.AllAreas, mNavMeshPath);

        if (mNavMeshPath.corners.Length < 1)
        {
            IsClearTargetPosition = true;
            return;
        }

        MoveEndCallBack = callback;

        //起点和当前位置不同时
        if (XUtility.DistanceNoY(mNavMeshPath.corners[0], transform.position) > NextPace || mNavMeshPath.corners.Length == 1)
        {
            ToCorner = 0;

            if (XUtility.DistanceNoY(mNavMeshPath.corners[0], transform.position) <= NextPace)
            {
                IsClearTargetPosition = true;
                transform.position = mNavMeshPath.corners[0];

                if (MoveEndCallBack != null)
                {
                    MoveEndCallBack();
                    MoveEndCallBack = null;
                }
                return;
            }
        }
        else
        {
            ToCorner = 1;

            if (XUtility.DistanceNoY(mNavMeshPath.corners[0], transform.position) <= NextPace)
                transform.position = mNavMeshPath.corners[0];
        }

        LeftLength = XUtility.DistanceNoY(transform.position, mNavMeshPath.corners[ToCorner]);

        //转向
        TurnToPos(mNavMeshPath.corners[ToCorner]);
    }

    protected Vector3 CommonMove()
    {
        NextPace = MovingSpeed * Time.deltaTime;

        if (XUtility.DistanceNoY(transform.position, NavMeshPosition) <= 0.1f
            || ToCorner == mNavMeshPath.corners.Length
            || (NextPace >= LeftLength && ToCorner == mNavMeshPath.corners.Length - 1))
        {
            if (!IsClearTargetPosition)
            {
                // 清除行走目标方向，停下，恢复Idle，到达目的地
                StopMoveToPoint();

                transform.position = NavMeshPosition;
                LeftLength = 0;

                if (MoveEndCallBack != null)
                {
                    MoveEndCallBack();
                    MoveEndCallBack = null;
                }
            }
        }
        else if (mNavMeshPath.corners.Length > 0 &&
            (XUtility.DistanceNoY(transform.position, mNavMeshPath.corners[ToCorner]) <= 0.1f || (NextPace > LeftLength)))
        {
            if (!IsClearTargetPosition)
            {
                //到达拐点
                transform.position = mNavMeshPath.corners[ToCorner];
                ToCorner++;

                if (ToCorner < mNavMeshPath.corners.Length)
                {
                    transform.LookAt(mNavMeshPath.corners[ToCorner]);
                    LeftLength = XUtility.DistanceNoY(transform.position, mNavMeshPath.corners[ToCorner]);
                }
            }
        }
        else if (!IsClearTargetPosition && ToCorner < mNavMeshPath.corners.Length)
        {
            // 继续走
            MovingDirection = XUtility.DirectionNoY(mNavMeshPath.corners[ToCorner] - transform.position).normalized;
        }

        return MovingSpeed * MovingDirection;
    }

    public Vector3 FixNavMeshPosition(Vector3 pos)
    {
        NavMeshHit closestHit;
        if (NavMesh.SamplePosition(pos, out closestHit, 500, NavMesh.AllAreas))
            return closestHit.position;
        else
            return pos;
    }

    public void TurnToPos(Vector3 position)
    {
        if (IsDead)
            return;

        position.y = transform.position.y;

        if (position == transform.position)
            return;

        transform.forward = (position - transform.position).normalized;
    }
    #endregion
}
