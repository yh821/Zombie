using UnityEngine;

public class EffectObject : SceneObject
{
    private float m_fDuration;
    private float m_fStartTime;
    private float m_fEndTime;

    private bool m_bIsDestroy;
    private bool m_bAutoDestroy;

    private bool m_bDelayDestroy;

    public bool CanActive()
    {
        return !m_bDelayDestroy || Time.time - m_fEndTime > 0.3f;
    }

    public void Begin(bool isAutoDestory)
    {
        m_fStartTime = Time.time;
        m_bIsDestroy = false;
        m_bAutoDestroy = isAutoDestory;
        EnterScene();
    }

    public void End()
    {
        m_bIsDestroy = true;
        m_fEndTime = Time.time;
    }

    public override void OnCreate()
    {
        ParticleSystem[] arrParticlesystems = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < arrParticlesystems.Length; i++)
        {
            ParticleSystem cCurParticle = arrParticlesystems[i];
            float fTime = cCurParticle.main.duration + cCurParticle.main.startLifetime.constantMax + cCurParticle.main.startDelay.constantMax;
            if (m_fDuration < fTime)
            {
                m_fDuration = fTime;
            }
        }

        TrailRenderer[] arrTrailRender = GetComponentsInChildren<TrailRenderer>();
        for (int i = 0; i != arrTrailRender.Length; ++i)
        {
            if (m_fDuration < arrTrailRender[i].time)
            {
                m_fDuration = arrTrailRender[i].time;
            }
        }
        m_bDelayDestroy = arrTrailRender.Length > 0;
    }

    public override void OnUpdate()
    {
        if (m_bAutoDestroy && !m_bIsDestroy && (Time.time - m_fStartTime >= m_fDuration))
        {
            EffectManager.Destory(gameObject);
        }
    }
}
