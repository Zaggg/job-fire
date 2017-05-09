using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace SubDis.Service
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new SubDisService() 
            };
            ServiceBase.Run(ServicesToRun);
        }


        //public static void Main()
        //{
        //    HostFactory.Run(x =>
        //    {
        //        x.Service<SubDisService>();
        //        //x.Service<SubDisService>(s =>
        //        //{
        //        //    s.SetServiceName("Stuff");
        //        //    s.ConstructUsing(name => new SubDisService());
        //        //    s.WhenStarted(tc => tc.Start());
        //        //    s.WhenStopped(tc => tc.Stop());
        //        //});
        //        //x.RunAsLocalSystem();

        //        //x.SetDescription("Sample Topshelf Host");
        //        //x.SetDisplayName("Stuff");
        //        //x.SetServiceName("Stuff");                       
        //    });
        //}
      

    }
}
