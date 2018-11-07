using UnityEngine;

public class GunItem : ItemObject
{
    public Transform FirePoint;

    public override void BePickup(ActorObject actor)
    {
        actor.AddItem(this);
    }
}

