using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputSys : CGameSystem
{
    private static InputSys m_cInstance;
    public static InputSys Instance
    {
        get { return m_cInstance; }
    }

    private Joystick mLeftJoystick;

    //private Joystick mRightJoystick;

    public override void SysInitial()
    {
        base.SysInitial();
        m_cInstance = this;
    }

    public override bool SysEnter()
    {
        UIMain ctrlFightUI = FightUISys.instance.uiCtrl;

        mLeftJoystick = new Joystick();
        mLeftJoystick.Init(UINodesManager.UICamera.WorldToScreenPoint(ctrlFightUI.JoystickPanel.position), 0.0f, Screen.width / 2.0f, true);

        //mRightJoystick = new Joystick();
        //mRightJoystick.Init(ctrlFightUI.JoystickPanel.position, 4.0f * Screen.width / 5.0f, Screen.width, false);

        mLeftJoystick.OnMoveListener += OnHeroMove;
        mLeftJoystick.OnMoveEndListener += OnHeroMoveEnd;

        //mRightJoystick.OnMoveListener += OnChangeSight;

        return base.SysEnter();
    }

    public override void SysUpdate()
    {
        mLeftJoystick.Update();
        //mRightJoystick.Update();
    }

    public override void SysLeave()
    {
        mLeftJoystick.Release();
        mLeftJoystick = null;

        //mRightJoystick.Release();
        //mRightJoystick = null;
    }

    private void OnHeroMove(Vector2 sDir)
    {
        Player cMainHero = GamingSys.Instance.Player;
        if (null == cMainHero)
        {
            return;
        }

        if (cMainHero.IsEnterScene)
        {
            if (!CMath.IsZero(sDir.y) || !CMath.IsZero(sDir.x))
            {
                Vector2 sMoveDir = ChaseCamera.instance.GetViewVector(sDir);
                cMainHero.Move(sMoveDir);
            }
        }
    }

    private void OnHeroMoveEnd()
    {
        Player cMainHero = GamingSys.Instance.Player;
        if (null == cMainHero)
        {
            return;
        }

        cMainHero.Stop();
        //cMainHero.SetFloat("runDirX", 0.0f);
        //cMainHero.SetFloat("runDirZ", 0.0f);

        //if (cMainHero.State != Actor.EState.Idle)
        //{
        //    CEventSys.Instance.TriggerEvent(new CSystemEvent(ESysEvent.OnHeroMoveEnd));
        //    cMainHero.State = Actor.EState.Idle;
        //}
    }

    private void OnChangeSight(Vector2 dir)
    {
        Player cMainHero = GamingSys.Instance.Player;
        if (null == cMainHero)
        {
            return;
        }

        if (cMainHero.IsEnterScene)
        {
            if (!CMath.IsZero(dir.x))
            {
                SceneCfg.instance.cameraRoot.Rotate(Vector3.up, dir.x, Space.World);
            }
        }
    }
}
