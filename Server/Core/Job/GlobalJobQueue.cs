using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class GlobalJobQueue : Singleton<GlobalJobQueue>
    {
        private Queue<Action> _jobs = new Queue<Action>();

        public static void PushJob(Action job) { Instance.Push(job); }

        public void Push(Action job)
        {
            lock (_jobs)            
                _jobs.Enqueue(job);            
        }

        public void Flush()
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
                    return null;

                return _jobs.Dequeue();
            }
        }
    }
}
