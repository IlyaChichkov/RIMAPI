using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RIMAPI
{
	public static class MainThreadDispatcher
	{
		private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
		private static int _maxPerTick = 512;

		public static void Enqueue(Action action)
		{
			if (action == null) return;
			_queue.Enqueue(action);
		}

		public static T Invoke<T>(Func<T> func)
		{
			if (func == null) throw new ArgumentNullException(nameof(func));
			T result = default(T);
			Exception captured = null;
			using (var evt = new ManualResetEventSlim(false))
			{
				Enqueue(() =>
				{
					try
					{
						result = func();
					}
					catch (Exception ex)
					{
						captured = ex;
					}
					finally
					{
						evt.Set();
					}
				});
				evt.Wait();
			}
			if (captured != null) throw captured;
			return result;
		}

		public static void PumpOnce()
		{
			int processed = 0;
			while (processed < _maxPerTick && _queue.TryDequeue(out var action))
			{
				try
				{
					action();
				}
				catch
				{
					// Intentionally swallow to avoid crashing the game loop; callers capture exceptions in Invoke
				}
				processed++;
			}
		}
	}
}


