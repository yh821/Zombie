using System;

namespace Base
{
    public abstract class Component : Disposer
    {
        public Entity Owner { get { return _owner; } set { _owner = value; } }
        private Entity _owner;

        public T GetOwner<T>() where T : Entity
        {
            return _owner as T;
        }

        public T GetComponent<T>() where T : Component
        {
            return Owner.GetComponent<T>();
        }

        protected Component()
        {
            //Game.EntityEventManager.Add(this);
        }

        protected Component(long id) : base(id)
        {
            //Game.EntityEventManager.Add(this);
        }

        public override void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }
            base.Dispose();
            _owner.RemoveComponent(GetType());
        }

        public static T CreateComponent<T>() where T : Component
        {
            Component comp;
            comp = Activator.CreateInstance<T>();
            return comp as T;
        }

        public virtual void Awake() { }

        public virtual void Update() { }
    }
}
