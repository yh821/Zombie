using UnityEngine;

public class Bullet : SceneObject
{
    private static RaycastHit[] mRaycasts = new RaycastHit[1];

    public LayerMask LayerMask;
    public Skill Skill;
    protected bool mIsTouch = false;//防止重复触发
    protected float mDeltaTime = 0;
    protected float mTime = 0.5f;
    protected Actor[] mTargets;

    public override void EnterScene()
    {
        base.EnterScene();

        int count = Physics.SphereCastNonAlloc(transform.position, 1f, transform.forward, mRaycasts, 2, LayerMask);
        if (count > 0)
        {
            mIsTouch = true;
            Actor actor = mRaycasts[0].transform.GetComponentInChildren<Actor>();
            if (null != actor)
            {
                actor.OnHurt(Skill);
            }
        }

        LeaveScene();
    }

    public virtual bool InAttackArea(out Actor[] actors)
    {
        actors = null;
        return false;
    }
}
