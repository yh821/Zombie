using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreLoadSys : CGameSystem 
{
    public override bool SysEnter()
    {
        return true;
    }

    public override IEnumerator SysEnterCo()
    {
        while (!SceneLoadSys.instance.isDone)
        {
            yield return null;
        }

        ActorManager.Cache(EActorType.Hero, 1002, 5);
        ActorManager.Cache(EActorType.Monster, 2001, 10);

        yield return null;

        CGameRoot.SwitchToState(EStateType.Gaming);
    }

    public override void SysLeave()
    {
        base.SysLeave();
    }
}
