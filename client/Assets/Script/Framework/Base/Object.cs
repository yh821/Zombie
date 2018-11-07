using System;

namespace Base
{
    public abstract class Object : IDisposable
    {
        public long Id { get; set; }

        protected Object()
        {
            Id = IdGenerater.GenerateId();
        }

        protected Object(long id)
        {
            Id = id;
        }

        public bool IsDisposed()
        {
            return this.Id == 0;
        }

        public virtual void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }

            this.Id = 0;
        }
    }
}
