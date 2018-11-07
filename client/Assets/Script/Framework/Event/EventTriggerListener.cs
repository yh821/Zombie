using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerListener : EventTrigger
{
    public Action<GameObject> onClick;
    public Action<GameObject> onDown;
    public Action<GameObject> onEnter;
    public Action<GameObject> onExit;
    public Action<GameObject> onUp;
    public Action<GameObject, bool> onHover;
    public Action<GameObject, PointerEventData> onDrag;
    public bool isExit;

    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>() ?? go.AddComponent<EventTriggerListener>();
        return listener;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (null != onClick)
        {
            onClick(gameObject);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (null != onDrag)
        {
            onDrag(gameObject, eventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (null != onDown)
        {
            onDown(gameObject);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (null != onHover)
        {
            onHover(gameObject, true);
        }
        if (null != onEnter)
        {
            onEnter(gameObject);
        }
        isExit = false;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (null != onHover)
        {
            onHover(gameObject, false);
        }
        if (null != onExit)
        {
            onExit(gameObject);
        }
        isExit = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!isExit && null != onUp)
        {
            onUp(gameObject);
        }

    }
}