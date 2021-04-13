using Paper.Model;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Paper.Co_location
{
    internal class InstanceSearchQueueManager<T> : IVisitor<T>
    {
        private readonly BlockingCollection<T> _instanceQueue;

        private readonly Task[] _tasks;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public InstanceSearchQueueManager(Configuration config, CancellationTokenSource cancellationToken)
        {
            this._instanceQueue = new BlockingCollection<T>(config.QueueMaxCapacity);
            this._tasks = new Task[config.TaskMaxCount];
            this._cancellationTokenSource = cancellationToken;
        }

        internal void Add(T t)
        {
            _instanceQueue.Add(t);
        }
        // 多个线程进行消费
        internal void Run(Action<IVisitor<T>> action)
        {
            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = Task.Run(() => action(this), _cancellationTokenSource.Token);
            }
        }


        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        internal void WaitAll() => Task.WaitAll(_tasks);

        internal void AddCompleted() => _instanceQueue.CompleteAdding();
        T IVisitor<T>.Take() => _instanceQueue.Take();
    }
}
