using UnityEngine;
using UnityEngine.UI;

public class UIHpBar : BaseUIBehaviour
{
    private Actor _actor;
    private Vector3 _screenPos;

    private uint mTimeId = 0;
    public float Value
    {
        get { return _hp.value; }
        set
        {
            _hp.value = value;
            if (value >= 1)
            {
                OnDisable();
                mTimeId = CTimeSys.Instance.AddTimer(3000, 0, () => { _actor.HpBarVisable = false; });
            }
            else
                OnDisable();
        }
    }

    private Slider _hp;
    private Image _fill;

    protected override void InitUI()
    {
        _hp = FindTransform("Bar").GetComponent<Slider>();
        _fill = FindTransform("Fill").GetComponent<Image>();
    }

    void OnDisable()
    {
        if (mTimeId > 0)
        {
            CTimeSys.Instance.DelTimer(mTimeId);
            mTimeId = 0;
        }
    }

    public void SetActor(Actor actor)
    {
        _actor = actor;
        if (actor is Hero)
            _fill.color = Color.green;
        else
            _fill.color = Color.red;
    }

    void LateUpdate()
    {
        _screenPos = SceneCfg.instance.cMainCamera.WorldToScreenPoint(_actor.position + new Vector3(0, 2.4f, 0));
        if (CheckPos(_screenPos))
        {
            _actor.HpBarVisable = true;
            transform.position = UINodesManager.UICamera.ScreenToWorldPoint(_screenPos);
        }
        else
            _actor.HpBarVisable = false;
    }

    private bool CheckPos(Vector3 pos)
    {
        if (pos.z > 60)//裁剪范围
            return false;
        if (pos.x < 0 || pos.x > Screen.width)
            return false;
        if (pos.y < 0 || pos.y > Screen.height)
            return false;
        return true;
    }
}
