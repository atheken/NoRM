using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

namespace NoRM
{
    /// <summary>
    /// Thanks to Eric Gunnerson and Phil Haack
    /// </summary>
    public struct TimedLock : IDisposable
    {
        private readonly object _target;
        private readonly Sentinel _leakDetector;

        private TimedLock(object o)
        {
            _target = o;
#if DEBUG
            _leakDetector = new Sentinel();
#endif
        }

        public static TimedLock Lock(object o)
        {
            return Lock(o, TimeSpan.FromSeconds(10));
        }

        public static TimedLock Lock(object o, TimeSpan timeout)
        {
            var tl = new TimedLock(o);

            if (!Monitor.TryEnter(o, timeout))
            {
#if DEBUG
                GC.SuppressFinalize(tl._leakDetector);
                StackTrace blockingTrace;
                lock (Sentinel.StackTraces)
                {
                    blockingTrace = Sentinel.StackTraces[o] as StackTrace;
                }
                throw new LockTimeoutException(blockingTrace);
#else
				throw new LockTimeoutException();
#endif
            }
#if DEBUG
            // Lock acquired. Store the stack trace.
            var trace = new StackTrace();
            lock (Sentinel.StackTraces)
            {
                Sentinel.StackTraces.Add(o, trace);
            }
#endif
            return tl;
        }

        public void Dispose()
        {
            Monitor.Exit(_target);

            // It's a bad error if someone forgets to call Dispose,
            // so in Debug builds, we put a finalizer in to detect
            // the error. If Dispose is called, we suppress the
            // finalizer.
#if DEBUG
            GC.SuppressFinalize(_leakDetector);
            lock (Sentinel.StackTraces)
            {
                Sentinel.StackTraces.Remove(_target);
            }
#endif
        }

#if DEBUG
        // (In Debug mode, we make it a class so that we can add a finalizer
        // in order to detect when the object is not freed.)
        private class Sentinel
        {
            public static readonly Hashtable StackTraces = new Hashtable();

            ~Sentinel()
            {
                // If this finalizer runs, someone somewhere failed to
                // call Dispose, which means we've failed to leave
                // a monitor!
                Debug.Fail("Undisposed lock");
            }
        }
#endif
    }

    #region public class LockTimeoutException : ApplicationException
    /// <summary>
    /// Thrown when a lock times out.
    /// </summary>
    [Serializable]
    public class LockTimeoutException : ApplicationException
    {
        public LockTimeoutException() : base("Timeout waiting for lock")
        {
        }

        public LockTimeoutException(string message) : base(message)
        { }

        public LockTimeoutException(string message, Exception innerException) : base(message, innerException)
        { }

#if DEBUG
        public LockTimeoutException(StackTrace blockingStackTrace)
        {
            BlockingStackTrace = blockingStackTrace;
        }
#endif

        protected LockTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

#if DEBUG
        public StackTrace BlockingStackTrace { get; private set; }
#endif
    }
    #endregion
}
