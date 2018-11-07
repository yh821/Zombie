using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XEngine
{
    /// <summary>
    /// 将BaseEventData对EventSystem的引用封装在底层
    /// </summary>
    public abstract class BaseCommand : BaseEventData
    {
        public BaseCommand()
            : base(CommandCenter.current)
        {
        }
    }

    /// <summary>
    /// 用来作为接收事件必须继承的接口，但是由于用了反射，所以接口内可以为空
    /// </summary>
    public interface ICommandReceiver : IEventSystemHandler
    {
    }

    /// <summary>
    /// 命令分发中心，使用UGUI的事件引擎
    /// </summary>
    public static class CommandCenter
    {
        public static EventSystem current = CGameRoot.Instance.GetComponentInChildren<EventSystem>();

        public static Dictionary<Type, MethodInfo[]> map_type_methodinfos = new Dictionary<Type, MethodInfo[]>();
        public static Dictionary<Type, MethodInfo> map_class_methodinfo = new Dictionary<Type, MethodInfo>();

        public static void ExecuteCommand(Transform t, BaseEventData data)
        {
            ExecuteEvents.Execute<ICommandReceiver>(t.gameObject, data, EventFunction);
        }

        public static void EventFunction(ICommandReceiver handler, BaseEventData eventData)
        {
            var parameterType = eventData.GetType();

            if (!map_class_methodinfo.ContainsKey(parameterType))
                map_class_methodinfo[parameterType] = typeof(Actor).GetFirstMethodsBySig(parameterType);

            map_class_methodinfo[parameterType].Invoke(handler, new object[] { eventData });
        }

        public static MethodInfo GetFirstMethodsBySig(this Type type, Type parameterType)
        {
            if (!map_type_methodinfos.ContainsKey(type))
                map_type_methodinfos[type] = type.GetMethods();

            var methods = map_type_methodinfos[type];
            for (int i = 0; i < methods.Length; i++)
            {
                var parameters = methods[i].GetParameters();

                for (int j = 0; j < parameters.Length; j++)
                    if (parameters[j].ParameterType == parameterType)
                        return methods[i];
            }

            return null;
        }
    }
}
