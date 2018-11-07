using UnityEngine;

public class TriggerCounter : MonoBehaviour
{
    private BoxCollider _box;

    void Start()
    {
        _box = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            int number;
            if (int.TryParse(gameObject.name, out number))
                EventDispatcher.Broadcast(EventNames.EnterTrigger, number, _box);
        }
    }
}
