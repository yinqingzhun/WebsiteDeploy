using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace AohPackageSubscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<List<IDaemonService>>(s =>
                {
                    s.ConstructUsing(name => new List<IDaemonService>()
                    {
                        new DeployMonitorService()
                    });
                    s.WhenStarted(tc => tc.ForEach(ds => ds.Start()));
                    s.WhenStopped(tc => tc.ForEach(ds => ds.Stop()));
                });


                x.RunAsLocalSystem();
                x.SetDescription("监控车友之家站点更新包的发布，自动备份和升级发布包");
                x.SetDisplayName("AohPackageSubscriber");
                x.SetServiceName("AohPackageSubscriber");
                x.StartAutomatically();
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(3);
                    r.OnCrashOnly();//should this be true for crashed or non-zero exits
                });

            });
        }
    }
}
