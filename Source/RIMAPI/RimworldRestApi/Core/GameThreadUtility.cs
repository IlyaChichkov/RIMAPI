using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RIMAPI.Core
{
    // 1. The hidden Unity object that runs on the main game thread
    public class GameThreadDispatcher : MonoBehaviour
    {
        // Thread-safe queue to hold actions coming from the HTTP server
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

        public void Update()
        {
            // Drain the queue every frame. 
            // We limit to 50 per frame so a massive API spam doesn't drop the game's FPS to zero.
            int processed = 0;
            while (processed < 50 && _executionQueue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Log.Error($"[RIMAPI] Error executing API action on main thread: {ex}");
                }
                processed++;
            }
        }

        public static void Enqueue(Action action)
        {
            _executionQueue.Enqueue(action);
        }

        internal static async Task<ApiResult<bool>> InvokeAsync(Func<ApiResult<bool>> value)
        {
            throw new NotImplementedException();
        }
    }

    // 2. The utility wrapper that gives us beautiful async/await syntax in our Controllers
    public static class GameThreadUtility
    {
        // For methods that return a value (e.g., getting a screenshot or pawn data)
        public static Task<T> InvokeAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            GameThreadDispatcher.Enqueue(() =>
            {
                try
                {
                    T result = func();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        // For methods that just execute an action (e.g., forcing a job, dropping an item)
        public static Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            GameThreadDispatcher.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}