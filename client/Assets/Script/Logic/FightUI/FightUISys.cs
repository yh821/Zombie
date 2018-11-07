using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightUISys : CGameSystem
{
    private static FightUISys m_cInstance;
    public static FightUISys instance
    {
        get { return m_cInstance; }
    }

    private UIMain m_cUICtrl = null;
    public UIMain uiCtrl
    {
        get { return m_cUICtrl; }
    }

    //private Gun m_cSingleGun;
    //private Gun m_cMachineGun;

    public override void SysInitial()
    {
        base.SysInitial();
        m_cInstance = this;
    }

    public override bool SysEnter()
    {
        GameObject cObj = UINodesManager.Instance.OpenUI("UIMain");
        if (null != cObj)
        {
            m_cUICtrl = cObj.AddUniqueComponent<UIMain>();
        }
        return false;
    }

    public override void SysLeave()
    {
        if (null != m_cUICtrl)
        {
            Destroy(m_cUICtrl.gameObject);
            m_cUICtrl = null;
        }

        base.SysLeave();
    }

    public void OnMainHeroInited()
    {
        //Player cMainHero = GamingSys.instance.player;
        //cMainHero.AddAttachItemListener(OnMainHeroAttachItem);
    }

    private void OnClickSingleGun(bool isOn)
    {
        if (isOn)
        {
            //Player cMainHero = GamingSys.instance.player;
            //if (null != m_cSingleGun)
            //{
            //    cMainHero.ChangeGun(m_cSingleGun);
            //}
        }
    }

    private void OnClickMachineGun(bool isOn)
    {
        if (isOn)
        {
            //Player cMainHero = GamingSys.instance.player;
            //if (null != m_cMachineGun)
            //{
            //    cMainHero.ChangeGun(m_cMachineGun);
            //}
        }
    }

    private void OnMainHeroAttachItem(ItemObject cItemObject)
    {
        //if (EItemType.ESingle == cItem.type)
        //{
        //    m_cSingleGun = (Gun)cItem;
        //}
        //else if (EItemType.EMachine == cItem.type)
        //{
        //    m_cMachineGun = (Gun)cItem;
        //}
    }
}
