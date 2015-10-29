using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDeploy.Utils;
namespace AohPackageSubscriber
{
    public class DeployMonitorService : IDaemonService
    {
        private const int interval = 3000;
        private string deployServiceUrl = string.Empty;
        private string tempDir = string.Empty;
        private const string tempFileName = "temp.zip";
        private string webSitePath = string.Empty;
        private string webSiteName = string.Empty;
        private string WebSiteDirectoryBackup = string.Empty;
        private string webSiteHealthMonitorURL = string.Empty;
        public void Start()
        {
            //初始化参数
            deployServiceUrl = string.Format("http://{0}/package/", AppConfigHelper.GetAppSetting("PackageDeployServer").ToString());
            tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            webSitePath = AppConfigHelper.GetAppSetting("WebSite.Path").ToString();
            webSiteName = AppConfigHelper.GetAppSetting("WebSite.Name").ToString();
            WebSiteDirectoryBackup = AppConfigHelper.GetAppSetting("WebSite.BackupLocation").ToString();
            webSiteHealthMonitorURL = AppConfigHelper.GetAppSetting("WebSite.HealthMonitorURL").ToString();

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
                Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                //检测站点文件是否需要更新

                //下载最新可用包文件
                DownloadDeployPackage();
                //解压包文件
                string tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempDir, tempFileName);
                ZipHelper.UnZipFile(tempFilePath, tempDir);
                //备份当前站点
                XCopy.Copy(webSitePath, Path.Combine(WebSiteDirectoryBackup, DateTime.Now.ToString("yyyy-MM-dd_hhmmss.fff")));
                //覆盖站点的包文件 TODO 提取站点文件
                XCopy.Copy(tempDir, webSitePath);
                //失败时，回滚到备份文件
                if (!IsWebsiteHealthy())
                {
                    //获取备份文件路径
                    string backupPath = GetNewBackupPackagePath();
                    IISHelper.SetWebSitePath(webSiteName, backupPath);
                    //回滚成功时，覆盖文件
                    XCopy.Copy(backupPath, webSitePath);

                    //通知服务端更新失败、回滚成功
                }
                else
                {
                    //通知服务端更新成功
                }


                Thread.Sleep(interval);
            }

        }
        /// <summary>
        /// 下载包文件
        /// </summary>
        private void DownloadDeployPackage()
        {
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
        }

        private bool IsWebsiteHealthy()
        {
            string s = HttpWebRequestHelper.Get(webSiteHealthMonitorURL);
            return "Status OK".Equals(s);
        }

        private string GetNewBackupPackagePath()
        {
            string[] dirList = Directory.GetDirectories(WebSiteDirectoryBackup);
            return dirList.ToList().Max();
        }
    }
}
