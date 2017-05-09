using SubDis.Service.Infos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubDis.Service
{
    public class ThreadPoolTools
    {
        public static int maxThreadWorker;
        private static int minThreadWorker;
        //ThreadPool.SetMaxThreads(3, 3);
        //private static object _lock = new object();

        //private static Queue<ThreadStart> threadStartQueue = new Queue<ThreadStart>();
        //private static HashSet<ThreadStart> threadsWorker = new HashSet<ThreadStart>();

        //ManualResetEvent[] events;

        //static ThreadPoolTools()
        //{
        //    maxThreadWorker = int.Parse(ConfigurationManager.AppSettings["threadcount"]);
        //    //events = new ManualResetEvent[maxThreadWorker];
        //}
        
        public static int GetMaxThreadsNum()
        {
            maxThreadWorker = int.Parse(PrimaryInfo.ThreadCount);//int.Parse(ConfigurationManager.AppSettings["threadcount"]); 
            return maxThreadWorker;//int.Parse(ConfigurationManager.AppSettings["threadcount"]); 
        }
        //public static ThreadPoolTools(int count)
        //{
        //    //events = new ManualResetEvent[count];
        //}

        public static void AddIntoPool()
        {
            
        }

        //public static void SetMaxThreadWorker(int max)
        //{
        //    maxThreadWorker = max;
        //}

        //public static void SetMinThreadWorker(int min)
        //{
        //    maxThreadWorker = min;
        //}

        //public static void AddSingleThread(ThreadStart ts)
        //{
        //    lock(_lock)
        //    {
        //        threadStartQueue.Enqueue(ts);
        //    }
        //}

        //public static void AddThreadsArray(ThreadStart[] tsArray)
        //{
        //    foreach(var ts in tsArray)
        //    {
        //        AddSingleThread(ts);
        //    }
        //}

    }
}
