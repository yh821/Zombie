using GameData;
using UnityEngine;

/// <summary>
/// Buff类
/// </summary>
public class Buff : Skill
{
    public Player player;

    public Buff(ResSkill data) : base(data)
    {
        _cd = Attr.AtkDuration / 1000f;
    }

    private float _deltaTime = 0;
    private float _cd;

    public override void Execute()
    {
        GamingSys.Instance.Player.AddHp(Attr.Hp, false);
    }

    public override void ShowEffect()
    {
    }

    public override void Update()
    {
        _deltaTime += Time.deltaTime;
        if (_deltaTime >= _cd)
        {
            _deltaTime = 0;
            Execute();
        }
    }
}
