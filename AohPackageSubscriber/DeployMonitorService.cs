using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WebDeploy.Utils;
namespace AohPackageSubscriber
{
    public class DeployMonitorService : IDaemonService
    {
        private const int interval = 3000;
        private string deployServiceUrl = string.Empty;
        private string tempDir = string.Empty;
        private const string tempFileName = "temp.zip";
        public void Start()
        {
            deployServiceUrl = string.Format("http://{0}/package/", AppConfigHelper.GetAppSetting("PackageDeployServer").ToString());
            tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");

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
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(deployServiceUrl);
                var response = req.GetResponse();
                string tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempDir, tempFileName);
                if (File.Exists(tempFilePath))
                    File.Delete(tempFileName);

                using (FileStream fs = new FileStream(tempFilePath, FileMode.CreateNew))
                {
                    byte[] bytes = new byte[1024];
                    int count = 0;
                    while ((count = response.GetResponseStream().Read(bytes, 0, bytes.Length)) > 0)
                    {
                        fs.Write(bytes, 0, count);
                    }
                }
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
