using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행 시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick;
        }
    }

    public class JobTimer : Singleton<JobTimer>
    {
        private PriorityQueue<JobTimerElem> _jobs = new PriorityQueue<JobTimerElem>();

        public static void Push(Action action, int interval)
        {
            Instance._Push(action, interval);
        }

        public static void Flush()
        {
            Instance._Flush();
        }

        private void _Push(Action action, int interval)
        {
            JobTimerElem job;
            job.action      = action;
            job.execTick    = Environment.TickCount + interval;

            lock (_jobs)
                _jobs.Push(job);
        }

        private void _Flush()
        {
            var nowTick = Environment.TickCount;
            while (true)
            {
                JobTimerElem job;

                lock (_jobs)
                {
                    if (0 == _jobs.Count)
                        return;

                    job = _jobs.Peek();                   
                    if (nowTick < job.execTick)
                        return;

                    _jobs.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
