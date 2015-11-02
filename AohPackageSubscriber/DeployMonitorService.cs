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
        private bool running = true;
        private const int interval = 3000;
        private string deployServiceHost = string.Empty;
        private string tempDir = string.Empty;
        private const string tempFileName = "temp.zip";
        private string webSitePath = string.Empty;
        private string webSiteName = string.Empty;
        private string WebSiteDirectoryBackup = string.Empty;
        private string webSiteHealthMonitorURL = string.Empty;
        private bool requireVerifiedForNewPackage = true;
        public void Start()
        {
            //初始化参数
            deployServiceHost = string.Format("http://{0}", AppConfigHelper.GetAppSetting("PackageDeployServer").ToString());
            tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            webSitePath = AppConfigHelper.GetAppSetting("WebSite.Path").ToString();
            webSiteName = AppConfigHelper.GetAppSetting("WebSite.Name").ToString();
            WebSiteDirectoryBackup = AppConfigHelper.GetAppSetting("WebSite.Directory.Backup").ToString();
            webSiteHealthMonitorURL = AppConfigHelper.GetAppSetting("WebSite.HealthMonitorURL").ToString();
            requireVerifiedForNewPackage = AppConfigHelper.GetAppSetting<bool>("NewPackage.RequireVerified", true);
            running = true;

            Thread t = new Thread(MonitorNewDeploy);
            t.Start();
        }

        public void Stop()
        {
            running = false;
        }



        private void MonitorNewDeploy()
        {
            while (running)
            {
                string uuid = string.Empty;
                try
                {


                    //检测站点文件是否需要更新
                    if (!NeedToDownloadDeployedPackage())
                    {
                        Thread.Sleep(5000);
                        continue;
                    }

                    //获取包文件uuid
                    uuid = GetNewestPackageUUId();
                    if (string.IsNullOrWhiteSpace(uuid))
                    {
                        Thread.Sleep(5000);
                        continue;
                    }
                    LogBeginingToUpdate(uuid);
                    //检查站点是否存在
                    if (!IISHelper.ExistWebsite(webSiteName))
                    {
                        Uri u = new Uri(webSiteHealthMonitorURL);
                        IISHelper.CreateWebsite(webSiteName, webSitePath, u.DnsSafeHost, u.Port);
                    }

                    //清理临时目录
                    if (Directory.Exists(tempDir))
                        DeleteDirectory(tempDir);
                    Directory.CreateDirectory(tempDir);
                    //下载最新可用包文件
                    if (!DownloadDeployPackage(uuid))
                    {
                        Thread.Sleep(5000);
                        continue;
                    }
                    LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + " 下载包文件完毕。");
                    //解压包文件
                    string tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempDir, tempFileName);
                    ZipHelper.UnZipFile(tempFilePath, tempDir);
                    LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + " 解压包文件完毕。");
                    //备份当前站点
                    string backup_exclude = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "backup_exclude.txt");
                    XCopy.Copy(webSitePath, Path.Combine(WebSiteDirectoryBackup, DateTime.Now.ToString("yyyy-MM-dd_hhmmss.fff")), backup_exclude);
                    LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + " 备份站点物理目录完毕。");
                    //覆盖站点的包文件 
                    string source_exclude = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "source_exclude.txt");
                    XCopy.Copy(GetLookLikeWebDeployDirectory(tempDir), webSitePath);
                    LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + " 覆盖站点物理目录完毕。");
                    IISHelper.SetWebSitePath(webSiteName, webSitePath);
                    LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + " 设置站点的物理路径为默认路径。");
                    //失败时，回滚到备份文件
                    if (!IsWebsiteHealthy())
                    {
                        LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + " 检测到站点无法正常访问。");
                        //站点切换到备份文件
                        string backupPath = GetNewBackupPackagePath();
                        IISHelper.SetWebSitePath(webSiteName, backupPath);
                        //回滚成功时，覆盖站点默认位置的文件，并将站点路径切换到默认位置
                        XCopy.Copy(backupPath, webSitePath);
                        IISHelper.SetWebSitePath(webSiteName, webSitePath);
                        LogUpdatingProgress(uuid, DateTime.Now.ToLocalTime() + string.Format(" 回滚站点完毕，站点{0}正常访问。", IsWebsiteHealthy() ? "可以" : "不能"));
                        LogFinishingUpdating(uuid, "更新站点后无法正常访问站点");

                    }
                    else
                    {
                        //通知服务端更新成功
                        LogFinishingUpdating(uuid);
                    }
                }
                catch (Exception ex)
                {
                    LogFinishingUpdating(uuid, ex.Message);
                    LogHelper.Error("包文件更新失败", ex);
                }

                Thread.Sleep(interval);
            }

        }
        private void LogBeginingToUpdate(string uuid)
        {
            HttpWebRequestHelper.Post(deployServiceHost + "/PackageReceive/BeginToReceive",
                new Dictionary<string, object>() { { "uuid", uuid }, { "hostName", Dns.GetHostName() } });
        }
        private void LogUpdatingProgress(string uuid, string msg)
        {
            HttpWebRequestHelper.Post(deployServiceHost + "/PackageReceive/UpdateReceiveInfo",
                new Dictionary<string, object>() { { "uuid", uuid }, { "msg", msg } });
        }
        private void LogFinishingUpdating(string uuid, string error = "")
        {
            HttpWebRequestHelper.Post(deployServiceHost + "/PackageReceive/FinishReceiving",
                new Dictionary<string, object>() { { "uuid", uuid }, { "error", error } });
        }

        private string GetNewestPackageUUId()
        {
            string s = HttpWebRequestHelper.Get(deployServiceHost + "/Deploy/GetNewDeployedPackageUUId?verified=" + requireVerifiedForNewPackage);
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            JObject o = JsonConvert.DeserializeObject<JObject>(s);
            if (o["status"].ToObject<int>() == 0)
            {
                return o["result"]["uuid"].ToString();
            }
            return string.Empty;
        }
        private bool NeedToDownloadDeployedPackage()
        {
            string s = HttpWebRequestHelper.Get(deployServiceHost + "/PackageReceive/NeedToDownloadDeployedPackage?hostName=" + Dns.GetHostName());
            if (string.IsNullOrWhiteSpace(s))
                return false;
            JObject o = JsonConvert.DeserializeObject<JObject>(s);
            if (o["status"].ToObject<int>() == 0)
            {
                return o["needToDownload"].ToObject<bool>();
            }
            return false;
        }
        /// <summary>
        /// 下载包文件
        /// </summary>
        private bool DownloadDeployPackage(string uuid)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(deployServiceHost + "/package/GetPackageFile?uuid=" + uuid);
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
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("下载包文件失败", ex);
                return false;
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
        /// <summary>
        /// 获取WebDeploy实际目录
        /// </summary>
        /// <param name="parentDir"></param>
        /// <returns></returns>
        private string GetLookLikeWebDeployDirectory(string parentDir)
        {
            string[] dirList = Directory.GetDirectories(parentDir);
            string[] fileList = Directory.GetFiles(parentDir);
            bool isOk = dirList.ToList().Any(p => "bin".Equals(Path.GetFileName(p), StringComparison.OrdinalIgnoreCase)) &&
                fileList.ToList().Any(p => "web.config".Equals(Path.GetFileName(p), StringComparison.OrdinalIgnoreCase));

            if (!isOk)
            {
                foreach (string dir in dirList)
                {
                    string target = string.Empty;
                    if (!string.IsNullOrWhiteSpace((target = GetLookLikeWebDeployDirectory(dir))))
                        return target;
                }
            }

            return isOk ? parentDir : string.Empty;

        }

        private void DeleteDirectory(string parentDir)
        {
            string[] dirList = Directory.GetDirectories(parentDir);
            string[] fileList = Directory.GetFiles(parentDir);
            Array.ForEach(fileList, p => File.Delete(p));
            Array.ForEach(dirList, p => DeleteDirectory(p));
            Directory.Delete(parentDir, true);
        }

    }
}
