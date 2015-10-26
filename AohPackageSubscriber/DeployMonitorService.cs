using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace AohPackageSubscriber
{
    public class DeployMonitorService:IDaemonService
    {
        private const int interval = 3000;
        public void Start()
        {
            Thread t = new Thread(MonitorNewDeploy);
            t.Start();
        }

        public void Stop()
        {
            
        }

        private void MonitorNewDeploy()
        {
            while (true)
            {
                //获取最新可用包文件

                //解压文件

                //备份文件1，2

                //复制文件

                //失败时，回滚到备份文件

                //回滚成功时，覆盖文件

                Thread.Sleep(interval);
            }

        }
    }
}
