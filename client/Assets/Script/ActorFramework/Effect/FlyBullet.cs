using AIMind.Nodes;
using UnityEngine;

public class FlyBullet : SceneObject
{
    private static RaycastHit[] mRaycasts = new RaycastHit[1];
    public LayerMask LayerMask;
    public Skill Skill;

    public Vector3 Forward
    {
        get { return mForward; }
        set
        {
            mForward = value;
            transform.forward = value;
        }
    }
    protected Vector3 mForward;

    protected bool mIsTouch;
    protected float mSpeed, mLimitDist;
    protected float mDeltaDist, mSumDist;
    protected Vector3 mLastPos, mCurPos;

    public override void EnterScene()
    {
        base.EnterScene();
        mSumDist = 0;
        mLimitDist = 20f;
        mSpeed = 60f;
        mIsTouch = false;
        mLastPos = mCurPos = transform.position;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        mDeltaDist = Time.deltaTime * mSpeed;
        if (mSumDist + mDeltaDist > mLimitDist)
        {
            mDeltaDist = mLimitDist - mSumDist;
        }
        mSumDist += mDeltaDist;
        mLastPos = mCurPos;
        mCurPos = mLastPos + Forward * mDeltaDist;
        transform.position = mCurPos;

        if (!mIsTouch)
        {
            int count = Physics.RaycastNonAlloc(mLastPos, Forward, mRaycasts, mDeltaDist, LayerMask);
            if (count > 0)
            {
                Actor actor = mRaycasts[0].transform.GetComponentInChildren<Actor>();
                if (null != actor)
                {
                    actor.OnHurt(Skill);
                    mIsTouch = true;
                    mLimitDist = mSumDist + mDeltaDist;
                }
            }
        }

        if (mSumDist >= mLimitDist)
            LeaveScene();
    }
}
