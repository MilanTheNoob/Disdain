using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    public static ThreadedDataRequester instance;

    static readonly List<Action> executeOnMainThread = new List<Action>();
    static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    static bool actionToExecuteOnMainThread = false;
    static Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    void Awake() { instance = this; }

    private void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        if (instance == null) instance = FindObjectOfType<ThreadedDataRequester>();

        void threadStart() { instance.DataThread(generateData, callback); }
        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
            return;

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }

    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
