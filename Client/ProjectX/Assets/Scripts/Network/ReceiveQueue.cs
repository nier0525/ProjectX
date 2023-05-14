using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveQueue : Singleton<ReceiveQueue>
{
    private struct TaskContext
    {
        private Action<PacketSession, ArraySegment<byte>> callback;
        private PacketSession       session;
        private ArraySegment<byte>  buffer;

        public TaskContext(Action<PacketSession, ArraySegment<byte>> callback, PacketSession session, ArraySegment<byte> buffer)
        {
            this.callback   = callback;
            this.session    = session;
            this.buffer     = buffer;
        }

        public void Invoke()
        {
            if (null != callback)
                callback.Invoke(session, buffer);
            else
                Debug.LogWarning("Invalid Task..");
        }
    }

    private Queue<TaskContext> tasks;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        tasks = new Queue<TaskContext>();
    }

    // Update is called once per frame
    void Update()
    {
        Flush();
    }

    public void Enqueue(Action<PacketSession, ArraySegment<byte>> callback, PacketSession session, ArraySegment<byte> buffer)
    {
        lock (tasks)
            tasks.Enqueue(new TaskContext(callback, session, buffer));
    }

    private void Flush()
    {
        lock (tasks)
        {
            if (0 == tasks.Count)
                return;

            var task = tasks.Dequeue();
            task.Invoke();
        }
    }
}
