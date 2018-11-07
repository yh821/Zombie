using UnityEngine;
using UnityEngine.UI;

public class UIMain : BaseUIBehaviour
{
    public Transform JoystickPanel { get; set; }

    private Text mTxBullet;
    private Text mTxFood;
    private Text mFPS;

    protected override void InitUI()
    {
        JoystickPanel = FindTransform("JoystickPanel").transform;
        mTxBullet = FindTransform("txBullet").GetComponent<Text>();
        mTxFood = FindTransform("txFood").GetComponent<Text>();
        mFPS = FindTransform("txFPS").GetComponent<Text>();

        FindTransform("btnSwitch").GetComponent<Button>().onClick.AddListener(OnSwitchWeapon);
    }

    protected override void AddListeners()
    {
        EventDispatcher.AddListener<int>(EventNames.ChangeBullet, OnChangeBullet);
        EventDispatcher.AddListener<int>(EventNames.ChangeFood, OnChangeFood);
    }

    protected override void RemoveListeners()
    {
        EventDispatcher.RemoveListener<int>(EventNames.ChangeBullet, OnChangeBullet);
        EventDispatcher.RemoveListener<int>(EventNames.ChangeFood, OnChangeFood);
    }

    void OnEnable()
    {
        OnChangeBullet(GamingSys.Instance.Player.Bullet);
        OnChangeFood(GamingSys.Instance.Player.Food);
    }

    private void OnChangeBullet(int value)
    {
        mTxBullet.text = value.ToString();
    }

    private void OnChangeFood(int value)
    {
        mTxFood.text = value.ToString();
    }

    void OnSwitchWeapon()
    {
        GamingSys.Instance.Player.ChangeWeapon();
    }

    void Update()
    {
        mFPS.text = CGameRoot.Instance.realFPS.ToString();
    }
}
