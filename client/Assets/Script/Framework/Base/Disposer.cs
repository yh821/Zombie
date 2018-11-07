namespace Base
{
    public abstract class Disposer : Object
    {
        protected Disposer() : base(IdGenerater.GenerateId())
        {
            //Game.Disposers.Add(this);
        }

        protected Disposer(long id) : base(id)
        {
            //Game.Disposers.Add(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            //Game.Disposers.Remove(this);
        }
    }
}
