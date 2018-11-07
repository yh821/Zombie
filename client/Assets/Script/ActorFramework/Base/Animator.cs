using UnityEngine;

public partial class SceneObject
{
    public Animator cAnimator { get; protected set; }
    private AnimationClip[] m_arrAnimClip;

    public void SetFloat(string strName, float fValue)
    {
        if (null != cAnimator)
        {
            cAnimator.SetFloat(strName, fValue);
        }
    }

    public void SetBool(string strName, bool bState)
    {
        if (null != cAnimator)
        {
            cAnimator.SetBool(strName, bState);
        }
    }

    public void SetInteger(string strName, int nValue)
    {
        if (null != cAnimator)
        {
            cAnimator.SetInteger(strName, nValue);
        }
    }

    public bool GetBool(string strName)
    {
        if (null != cAnimator)
        {
            return cAnimator.GetBool(strName);
        }
        return false;
    }

    public void SetTrigger(string strName)
    {
        if (null != cAnimator && !string.IsNullOrEmpty(strName))
        {
            cAnimator.SetTrigger(strName);
        }
    }

    public float GetLength(string strName)
    {
        for (int i = 0; i != m_arrAnimClip.Length; ++i)
        {
            AnimationClip cClip = m_arrAnimClip[i];
            if (strName == cClip.name)
            {
                return cClip.length;
            }
        }
        return 0.0f;
    }

    public void Play(string stateName)
    {
        if (null != cAnimator)
        {
            cAnimator.Play(stateName);
        }
    }
}
