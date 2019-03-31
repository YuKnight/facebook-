using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Xzy.EmbeddedApp.Model;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class SimulationTaskManager
    {
        const int MaxClearNum = 60;

        private static readonly ConcurrentDictionary<int, ISimulationTask> Tasks;

        private static readonly object SyncRoot = new object();

        private static readonly Timer Timer;

        private static int _runClear;
        private static int _clearNum;

        static SimulationTaskManager()
        {
            Tasks = new ConcurrentDictionary<int, ISimulationTask>();
            Timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public static bool RegisterSingle(ISimulationTask task)
        {
                if (Tasks.ContainsKey(task.Id))
                {
                    return false;
                }
                else
                {
                    Tasks.GetOrAdd(task.Id, task);
                    _clearNum = 0;
                    Timer.Change(1000, Timeout.Infinite);
                    return true;
                }        
        }


        public static void Invoke<T>(int id, T result)
        {
            ISimulationTask task;
            if (Tasks.TryGetValue(id, out task))
            {
                task.SetResult(result);
            } 
        }


        static void TimerCallback(object state)
        {
            if (Interlocked.CompareExchange(ref _runClear, 1, 0) == 0)
            {
                RunClear();
                _runClear = 0;
                Timer.Change(_clearNum > MaxClearNum ? Timeout.Infinite : 1000, Timeout.Infinite);
            }
        }

        private static void RunClear()
        {
            int removed = 0;

            List<ISimulationTask> tobeRemovedTasks = new List<ISimulationTask>();

            var now = DateTime.Now.GetTimestamp();

            foreach (var task in Tasks.Values)
            {
                if (task.Task.IsCompleted || task.Task.IsCanceled || task.Task.IsFaulted)
                {
                    tobeRemovedTasks.Add(task);
                }
                else if (task.Timeout > 0 && task.StartTime + task.Timeout < now)
                {
                    task.SetException(new TimeoutException("任务超时"));
                    tobeRemovedTasks.Add(task);
                }
            }

            foreach (var tobeRemovedTask in tobeRemovedTasks)
            {
                removed++;
                ISimulationTask t;
                bool removedResult = Tasks.TryRemove(tobeRemovedTask.Id, out t);
                Console.WriteLine($"remove task result:{removedResult}");
            }

            if (removed == 0)
            {
                _clearNum++;
            }
        }
    }
}
