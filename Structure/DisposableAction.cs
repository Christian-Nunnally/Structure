using System;

namespace Structure.Structure
{
    public class DisposableAction : IDisposable
    {
        public DisposableAction() : this(null, null) { }

        public DisposableAction(Action dispose) : this(null, dispose) { }

        public DisposableAction(Action construct, Action dispose)
        {
            construct?.Invoke();
            DisposeAction = dispose;
        }

        protected Action DisposeAction { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (DisposeAction == null) return;
            DisposeAction?.Invoke();
            DisposeAction = null;
        }
    }
}
