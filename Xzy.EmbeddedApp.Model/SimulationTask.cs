using System;
using System.Threading.Tasks;
using System.Threading;

namespace Xzy.EmbeddedApp.Model
{
    public interface ISimulationTask
    {
        long StartTime { get; }
        uint Timeout { get; }
        int Id { get; }

        void SetResult(object result);
        void SetException(Exception e);

        Task Task { get; }
    }


    public class SimulationTask<TResult> : ISimulationTask
    {
        public long StartTime { get; }

        public uint Timeout { get; }

        public int Id { get; }

        public Task Task => _tcs.Task;


        private int _setResultFlag = 0;
        private readonly TaskCompletionSource<TResult> _tcs;



        public void SetException(Exception e)
        {
            if (Interlocked.CompareExchange(ref _setResultFlag,1,0) == 0)
            {
                _tcs.SetException(e);
            }
        }


        private void SetResult(TResult result)
        {
            if (Interlocked.CompareExchange(ref _setResultFlag,1,0) == 0)
            {
                _tcs.SetResult(result);
            }
        }

        public void SetResult(object result)
        {
            TResult temp = (TResult)result;

            if (temp == null)
            {
                SetException(new InvalidCastException("转换任务返回结果失败"));
            }
            else
            {
                SetResult(temp);
            }
        }


        public SimulationTask(int id, uint timeout)
        {
            Id = id;
            Timeout = timeout;
            StartTime = DateTime.Now.GetTimestamp();

            _tcs = new TaskCompletionSource<TResult>();
        }
    }
}
