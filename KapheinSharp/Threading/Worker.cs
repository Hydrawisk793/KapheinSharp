using System;
using System.Threading;

namespace KapheinSharp.Threading
{
    public class Worker
        : IDisposable
    {
        public class WorkerEventArgs
            : EventArgs
        {
            internal WorkerEventArgs(
                Worker worker
            )
                : this(worker, null)
            {
                
            }

            internal WorkerEventArgs(
                Worker worker
                , object parameter
            )
            {
                worker_ = worker;
                parameter_ = parameter;
            }

            public Worker Worker
            {
                get
                {
                    return worker_;
                }
            }

            public object Parameter
            {
                get
                {
                    return parameter_;
                }

                internal set
                {
                    parameter_ = value;
                }
            }

            private Worker worker_;

            private object parameter_;
        }

        public Worker()
        {
            worker_ = null;
            finishResetEvent_ = new ManualResetEvent(false);
            eventArgs_ = null;
            thisLock_ = new object();
            isWorking_ = false;
            isLooping_ = false;
        }

        ~Worker()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsWorking
        {
            get
            {
                lock(thisLock_) {
                    return isWorking_;
                }
            }

            private set
            {
                lock(thisLock_) {
                    isWorking_ = value;
                }
            }
        }

        public void Start()
        {
            Start(null);
        }

        public void Start(
            object parameter
        )
        {
            lock(thisLock_) {
                if(!isWorking_) {
                    worker_ = new Thread(WorkerThreadMain);

                    isLooping_ = true;
                    isWorking_ = true;
                    finishResetEvent_.Reset();

                    eventArgs_ = new WorkerEventArgs(this, parameter);
                    worker_.Start();
                }
            }
        }

        public void Stop()
        {
            IsLooping = false;
        }

        public void Stop(
            int milliseconds
        )
        {
            Stop();

            WaitForFinish(milliseconds);
        }

        public bool WaitForFinish(
            int milliseconds
        )
        {
            var isResetEventSet = false;
            if(IsWorking) {
                isResetEventSet = finishResetEvent_.WaitOne((milliseconds < 0 ? Timeout.Infinite : milliseconds));
            }

            return isResetEventSet;
        }

        public event EventHandler<WorkerEventArgs> Begun;
        
        public event EventHandler<WorkerEventArgs> Working;

        public event EventHandler<WorkerEventArgs> Ended;
        
        protected void Dispose(
            bool canDisposeManagedResources
        )
        {
            Stop();
            
            if(canDisposeManagedResources) {
                WaitForFinish(-1);
            }
        }

        protected void OnBegun(
            WorkerEventArgs e
        )
        {
            var del = Begun;
            if(del != null) {
                del(this, e);
            }
        }

        protected void OnWorking(
            WorkerEventArgs e
        )
        {
            var del = Working;
            if(del != null) {
                del(this, e);
            }
        }

        protected void OnEnded(
            WorkerEventArgs e
        )
        {
            var del = Ended;
            if(del != null) {
                del(this, e);
            }
        }

        private void WorkerThreadMain()
        {
            OnBegun(eventArgs_);
            
            while(ShouldLoop) {
                OnWorking(eventArgs_);
            }
            
            OnEnded(eventArgs_);

            finishResetEvent_.Set();

            IsWorking = false;
        }

        private bool IsLooping
        {
            get
            {
                lock(thisLock_) {
                    return isLooping_;
                }
            }

            set
            {
                lock(thisLock_) {
                    isLooping_ = value;
                }
            }
        }

        private bool ShouldLoop
        {
            get
            {
                lock(thisLock_) {
                    return isWorking_ && isLooping_;
                }
            }
        }

        private Thread worker_;

        private ManualResetEvent finishResetEvent_;

        private WorkerEventArgs eventArgs_;

        private object thisLock_;

        private volatile bool isWorking_;

        private volatile bool isLooping_;
    }
}
