using System;
using System.Collections.Generic;
using System.Linq;

namespace Base
{
    public abstract class Entity : Disposer
    {
        private HashSet<Component> components = new HashSet<Component>();
        private Dictionary<Type, Component> componentDict = new Dictionary<Type, Component>();

        protected Entity()
        {
            //EntityWorld.Instance.AllEntities.Add(Id, this);
        }

        protected Entity(long id) : base(id)
        {
            //EntityWorld.Instance.AllEntities.Add(Id, this);
        }

        public override void Dispose()
        {
            if (Id == 0)
            {
                return;
            }
            base.Dispose();
            Component[] arrComponents = GetComponents();
            for (int i = 0; i != arrComponents.Length; ++i)
            {
                Component component = arrComponents[i];
                try
                {
                    component.Dispose();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                }
            }
            //EntityWorld.Instance.AllEntities.Remove(Id);

            components.Clear();
            componentDict.Clear();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            try
            {
                if (this.componentDict.ContainsKey(typeof(T)))
                {
                    return GetComponent<T>();
                }
                T component = Component.CreateComponent<T>();
                component.Owner = this;

                this.components.Add(component);
                this.componentDict.Add(typeof(T), component);
                component.Awake();
                return component;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                return null;
            }
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component component;
            if (!this.componentDict.TryGetValue(typeof(T), out component))
            {
                return;
            }
            this.components.Remove(component);
            this.componentDict.Remove(typeof(T));
            component.Dispose();
        }

        public void RemoveComponent(Type type)
        {
            Component component;
            if (!this.componentDict.TryGetValue(type, out component))
            {
                return;
            }
            this.components.Remove(component);
            this.componentDict.Remove(type);
            component.Dispose();
        }

        public K GetComponent<K>() where K : Component
        {
            Component component;
            if (!this.componentDict.TryGetValue(typeof(K), out component))
            {
                return default(K);
            }
            return (K)component;
        }

        public Component[] GetComponents()
        {
            return components.ToArray();
        }

        public virtual void Update()
        {
            foreach (Component component in components)
            {
                component.Update();
            }
        }
    }
}
