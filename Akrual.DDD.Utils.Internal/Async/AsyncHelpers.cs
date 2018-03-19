using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Akrual.DDD.Utils.Internal.Async
{
    /// <summary>
	/// AsyncHelpers to simplify calling of Async/Task methods from synchronous context.
	/// </summary>
	public static class AsyncHelpers
	{
		/// <summary>
		///     Execute's an async Task<T> method which has a void return value synchronously</T>
		/// </summary>
		/// <param name="task">
		///     Task<T> method to execute</T>
		/// </param>
		/// <exception cref="AggregateException">AsyncHelpers.Run method threw an exception.</exception>
		/// <exception cref="AbandonedMutexException">The wait completed because a thread exited without releasing a mutex. This exception is not thrown on Windows 98 or Windows Millennium Edition.</exception>
		/// <exception cref="Exception">A delegate callback throws an exception.</exception>
		/// <exception cref="InvalidOperationException">The current instance is a transparent proxy for a <see cref="T:System.Threading.WaitHandle" /> in another application domain.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="innerException" /> argument is null.</exception>
		/// <exception cref="ObjectDisposedException">The current instance has already been disposed. </exception>
		public static void RunSync(Func<Task> task)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			synch.Post(async _ =>
			{
				try
				{
					await task().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();

			SynchronizationContext.SetSynchronizationContext(oldContext);
		}

		/// <summary>
		///     Execute's an async Task<T> method which has a T return type synchronously</T>
		/// </summary>
		/// <typeparam name="T">Return Type</typeparam>
		/// <param name="task">
		///     Task<T> method to execute</T>
		/// </param>
		/// <returns></returns>
		/// <exception cref="ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
		/// <exception cref="Exception">Error Executing Task Async.</exception>
		/// <exception cref="AbandonedMutexException">The wait completed because a thread exited without releasing a mutex. This exception is not thrown on Windows 98 or Windows Millennium Edition.</exception>
		/// <exception cref="AggregateException">AsyncHelpers.Run method threw an exception.</exception>
		/// <exception cref="InvalidOperationException">The current instance is a transparent proxy for a <see cref="T:System.Threading.WaitHandle" /> in another application domain.</exception>
		/// <exception cref="ArgumentNullException">The <paramref name="innerException" /> argument is null.</exception>
		public static T RunSync<T>(Func<Task<T>> task)
		{
			var oldContext = SynchronizationContext.Current;
			var synch = new ExclusiveSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext(synch);
			var ret = default(T);
			synch.Post(async _ =>
			{
				try
				{
					ret = await task().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					synch.InnerException = e;
					throw;
				}
				finally
				{
					synch.EndMessageLoop();
				}
			}, null);
			synch.BeginMessageLoop();
			SynchronizationContext.SetSynchronizationContext(oldContext);
			return ret;
		}

		private class ExclusiveSynchronizationContext : SynchronizationContext
		{
			private readonly Queue<Tuple<SendOrPostCallback, object>> _items =
				new Queue<Tuple<SendOrPostCallback, object>>();

			private readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);

			private bool _done;
			public Exception InnerException { get; set; }

			/// <exception cref="NotSupportedException">We cannot send to our same thread</exception>
			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("We cannot send to our same thread");
			}

			/// <exception cref="ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
			public override void Post(SendOrPostCallback d, object state)
			{
				lock (_items)
				{
					_items.Enqueue(Tuple.Create(d, state));
				}
				_workItemsWaiting.Set();
			}

			/// <exception cref="ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
			public void EndMessageLoop()
			{
				Post(_ => _done = true, null);
			}

			/// <exception cref="AggregateException">AsyncHelpers.Run method threw an exception.</exception>
			/// <exception cref="ObjectDisposedException">The current instance has already been disposed. </exception>
			/// <exception cref="InvalidOperationException">The current instance is a transparent proxy for a <see cref="T:System.Threading.WaitHandle" /> in another application domain.</exception>
			/// <exception cref="AbandonedMutexException">The wait completed because a thread exited without releasing a mutex. This exception is not thrown on Windows 98 or Windows Millennium Edition.</exception>
			/// <exception cref="Exception">A delegate callback throws an exception.</exception>
			/// <exception cref="ArgumentNullException">The <paramref name="innerException" /> argument is null.</exception>
			public void BeginMessageLoop()
			{
				while (!_done)
				{
					Tuple<SendOrPostCallback, object> task = null;
					lock (_items)
					{
						if (_items.Count > 0)
						{
							task = _items.Dequeue();
						}
					}
					if (task != null)
					{
						task.Item1(task.Item2);
						if (InnerException != null) // the method threw an exeption
						{
							throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
						}
					}
					else
					{
						_workItemsWaiting.WaitOne();
					}
				}
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}
		}
	}
}