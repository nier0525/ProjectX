using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IJobQueue
    {
        public void Push(Action job);
    }

    public class JobQueue : IJobQueue
    {
        private Queue<Action>   _jobs       = new Queue<Action>();
        private bool            _isFlush    = false;

        public void Push(Action job)
        {
            bool isFlush = false;
            lock (_jobs)
            {
                _jobs.Enqueue(job);
                if (false == _isFlush)
                    isFlush = _isFlush = true;
            }

            if (true == isFlush)
                Flush();
        }

        private void Flush()
        {
            while (true)
            {
                var job = Pop();
                if (null == job)
                    break;

                job.Invoke();
            }
        }

        private Action Pop()
        {
            lock (_jobs)
            {
                if (0 == _jobs.Count)
                {
                    _isFlush = false;
                    return null;
                }
                return _jobs.Dequeue();
            }
        }
    }
}
