using UnityEngine;

public class UIDepth : MonoBehaviour
{
    public int order = 1;
    void Start()
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer render in renders)
        {
            render.sortingOrder = order;
        }
    }
}