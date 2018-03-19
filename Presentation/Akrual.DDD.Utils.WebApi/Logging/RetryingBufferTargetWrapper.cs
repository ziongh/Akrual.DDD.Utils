using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NLog.Common;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Akrual.DDD.Utils.WebApi.Logging
{
    /// <summary>Retries in case of write error.</summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/RetryingWrapper-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes each write attempt to be repeated 3 times,
    /// sleeping 1 second between attempts if first one fails.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/RetryingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/RetryingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("RetryingWrapper", IsWrapper = true)]
    public class RetryingBufferTargetWrapper : WrapperTargetBase
    {
        /// <summary>
        /// Special SyncObject to allow closing down Target while busy retrying
        /// </summary>
        private readonly object RetrySyncObject = new object();

        /// <summary>
        /// Gets or sets the number of retries that should be attempted on the wrapped target in case of a failure.
        /// </summary>
        /// <docgen category="Retrying Options" order="10" />
        [DefaultValue(3)]
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the time to wait between retries in milliseconds.
        /// </summary>
        /// <docgen category="Retrying Options" order="10" />
        [DefaultValue(100)]
        public int RetryDelayMilliseconds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.Wrappers.RetryingTargetWrapper" /> class.
        /// </summary>
        public RetryingBufferTargetWrapper()
            : this((Target)null, 3, 100)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.Wrappers.RetryingTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        public RetryingBufferTargetWrapper(string name, Target wrappedTarget, int retryCount, int retryDelayMilliseconds)
            : this(wrappedTarget, retryCount, retryDelayMilliseconds)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NLog.Targets.Wrappers.RetryingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        public RetryingBufferTargetWrapper(Target wrappedTarget, int retryCount, int retryDelayMilliseconds)
        {
            this.WrappedTarget = wrappedTarget;
            this.RetryCount = retryCount;
            this.RetryDelayMilliseconds = retryDelayMilliseconds;
        }

        /// <summary>
        /// Writes the specified log event to the wrapped target, retrying and pausing in case of an error.
        /// </summary>
        /// <param name="logEvents">The log event.</param>
        protected override void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            lock (this.RetrySyncObject)
            {
                var logsToLog = UpdateContinuationOfLogEvents(logEvents);

                WriteToRegisteredTarget(logsToLog);
            }
        }

        private void WriteToRegisteredTarget(List<AsyncLogEventInfo> logsToLog)
        {
            lock (this.RetrySyncObject)
            {
                this.WrappedTarget.WriteAsyncLogEvents(logsToLog.ToArray());
            }
        }

        private List<AsyncLogEventInfo> UpdateContinuationOfLogEvents(IList<AsyncLogEventInfo> logEvents)
        {
            lock (this.RetrySyncObject)
            {
                List<AsyncLogEventInfo> logsToLog = new List<AsyncLogEventInfo>();
                for (var counter = 0; counter < logEvents.Count; ++counter)
                {
                    if (!this.IsInitialized)
                        logEvents[counter].Continuation((Exception) null);
                    else
                    {
                        var logEvent = logEvents[counter];
                        var continuation = GetContinuation(logEvent);
                        var logEventWithcontinuation = logEvent.LogEvent.WithContinuation(continuation);
                        logsToLog.Add(logEventWithcontinuation);
                    }
                }
                return logsToLog;
            }
        }

        private AsyncContinuation GetContinuation(AsyncLogEventInfo logEvent)
        {
            AsyncContinuation continuation = (AsyncContinuation) null;
            int counter = 0;
            continuation = (AsyncContinuation) (ex =>
            {
                if (ex == null)
                {
                    logEvent.Continuation((Exception) null);
                }
                else
                {
                    int num1 = Interlocked.Increment(ref counter);
                    InternalLogger.Warn("Error while writing to '{0}': {1}. Try {2}/{3}", (object) this.WrappedTarget,
                        (object) ex, (object) num1, (object) this.RetryCount);
                    if (num1 >= this.RetryCount)
                    {
                        InternalLogger.Warn("Too many retries. Aborting.");
                        logEvent.Continuation(ex);
                    }
                    else
                    {
                        int num2 = 0;
                        while (num2 < this.RetryDelayMilliseconds)
                        {
                            int millisecondsTimeout = Math.Min(100, this.RetryDelayMilliseconds - num2);
                            Thread.Sleep(millisecondsTimeout);
                            num2 += millisecondsTimeout;
                            if (!this.IsInitialized)
                            {
                                InternalLogger.Warn("Target closed. Aborting.");
                                logEvent.Continuation(ex);
                                return;
                            }
                        }
                        this.WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
                    }
                }
            });
            return continuation;
        }
    }
}