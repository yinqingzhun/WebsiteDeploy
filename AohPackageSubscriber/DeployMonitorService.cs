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
        private string websitePath = string.Empty;
        private string websiteName = string.Empty;
        private string websiteDirectoryBackup = string.Empty;
        private string websiteHealthMonitorURL = string.Empty;
        private bool requireVerifiedForNewPackage = true;
        private string backup_exclude = string.Empty;
        private string source_exclude = string.Empty;
        public void Start()
        {
            //初始化参数
            deployServiceHost = string.Format("http://{0}", AppConfigHelper.GetAppSetting("PackageDeployServer").ToString());
            tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            websitePath = AppConfigHelper.GetAppSetting("WebSite.Path").ToString();
            websiteName = AppConfigHelper.GetAppSetting("WebSite.Name").ToString();
            websiteDirectoryBackup = AppConfigHelper.GetAppSetting("WebSite.Directory.Backup").ToString();
            websiteHealthMonitorURL = AppConfigHelper.GetAppSetting("WebSite.HealthMonitorURL").ToString();
            requireVerifiedForNewPackage = AppConfigHelper.GetAppSetting<bool>("NewPackage.RequireVerified", true);
            running = true;
            backup_exclude = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "backup_exclude.txt");
            source_exclude = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "source_exclude.txt");
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
                int logId = 0;
                try
                {
                    //检测站点文件是否需要更新
                    if (!NeedToDownloadDeployedPackage())
                    {
                        Thread.Sleep(interval);
                        continue;
                    }

                    //获取包文件uuid
                    uuid = GetNewestPackageUUId();
                    if (string.IsNullOrWhiteSpace(uuid))
                    {
                        Thread.Sleep(interval);
                        continue;
                    }
                    logId = LogBeginingToUpdate(uuid);
                    //检查站点是否存在
                    bool existWebsite = IISHelper.ExistWebsite(websiteName);
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" {0}检测到站点{1}。", existWebsite ? "" : "未", websiteName));
                    if (!existWebsite)
                    {
                        Uri u = new Uri(websiteHealthMonitorURL);
                        IISHelper.CreateWebsite(websiteName, websitePath, u.Port);
                        if (IISHelper.CreateAppPool(websiteName))
                            IISHelper.SetAppPoolForWebsite(websiteName, websiteName);
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 创建站点{0}。", websiteName));
                    }

                    //清理临时目录
                    if (Directory.Exists(tempDir))
                        DeleteDirectory(tempDir);
                    Directory.CreateDirectory(tempDir);
                    //下载最新可用包文件
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 开始下载包文件。");
                    if (!DownloadDeployPackage(uuid))
                        throw new Exception("下载文件失败");
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 下载包文件完毕。");
                    //解压包文件
                    string tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempDir, tempFileName);
                    ZipHelper.UnZipFile(tempFilePath, tempDir);
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 解压包文件完毕。");
                    //停止当前站点
                    bool b = IISHelper.StopWebSitePath(websiteName);
                    Thread.Sleep(1000);
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 停止站点{0}。", b ? "成功" : "失败"));
                    //备份当前站点
                    string backup_dir = Path.Combine(websiteDirectoryBackup, DateTime.Now.ToString("yyyy-MM-dd_HHmmss.fff"));
                    if (!XCopy.Copy(websitePath, backup_dir, backup_exclude))
                        throw new Exception("备份当前站点时复制文件失败");
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 备份站点物理目录完毕。");
                    //站点切换到备份目录
                    bool change = IISHelper.SetWebSitePath(websiteName, backup_dir);
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 站点切换到备份目录{0}。", change ? "成功" : "失败"));
                    //覆盖站点的包文件 
                    string source_exclude = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "source_exclude.txt");
                    if (!XCopy.Copy(GetLookLikeWebDeployDirectory(tempDir), websitePath, source_exclude))
                        throw new Exception("覆盖站点的包文件时复制文件失败");
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 覆盖站点物理目录完毕。");
                    //站点切换到默认目录
                    change = IISHelper.SetWebSitePath(websiteName, websitePath);
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 站点切换到默认路径{0}。", change ? "成功" : "失败"));
                    //启动当前站点
                    b = IISHelper.StartWebSitePath(websiteName);
                    LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 启动站点{0}。", b ? "成功" : "失败"));
                    //失败时，回滚到备份文件
                    if (!IsWebsiteHealthy(logId))
                    {
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 检测到站点无法正常访问。");
                        //停止当前站点
                        b = IISHelper.StopWebSitePath(websiteName);
                        Thread.Sleep(1000);
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 停止站点{0}。", b ? "成功" : "失败"));
                        ////站点切换到备份文件
                        string backupPath = GetNewBackupPackagePath();
                        //change = IISHelper.SetWebSitePath(websiteName, backupPath);
                        //LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 站点切换到备份目录{0}。", change ? "成功" : "失败"));
                        //回滚成功时，覆盖站点默认位置的文件，并将站点路径切换到默认位置
                        XCopy.Copy(backupPath, websitePath);
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 恢复站点备份文件完毕。");
                        //启动当前站点
                        b = IISHelper.StartWebSitePath(websiteName);
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 启动站点{0}。", b ? "成功" : "失败"));

                        change = IISHelper.SetWebSitePath(websiteName, websitePath);
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 站点切换到默认路径{0}。", change ? "成功" : "失败"));
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + string.Format(" 回滚站点完毕，站点{0}正常访问。", IsWebsiteHealthy(logId) ? "可以" : "不能"));
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 更新站点失败。");
                        LogFinishingUpdating(logId, "更新站点后无法正常访问站点");

                    }
                    else
                    {
                        //通知服务端更新成功
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 检测到站点可以正常访问。");
                        LogUpdatingProgress(logId, DateTime.Now.ToLocalTime() + " 更新站点成功。");
                        LogFinishingUpdating(logId);
                    }
                }
                catch (Exception ex)
                {
                    if (logId > 0)
                    {
                        LogFinishingUpdating(logId, ex.Message);
                        LogHelper.Error("包文件更新失败", ex);
                    }

                }
                finally
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(uuid))
                            IISHelper.StartWebSitePath(websiteName);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.ToString());
                    }
                    uuid = string.Empty;
                    logId = 0;
                }

                Thread.Sleep(interval);
            }

        }
        private int LogBeginingToUpdate(string uuid)
        {
            LogHelper.Info(string.Format("开始更新站点{0}，更新包的UUID为{1}。", websiteName, uuid));
            string s = HttpWebRequestHelper.Post(deployServiceHost + "/PackageReceive/BeginToReceive",
                   new Dictionary<string, object>() { { "uuid", uuid }, { "hostName", Dns.GetHostName() } });
            if (string.IsNullOrWhiteSpace(s))
                return 0;
            JObject o = JsonConvert.DeserializeObject<JObject>(s);
            if (o["status"].ToObject<int>() == 0)
            {
                return o["result"]["logId"].ToObject<int>();
            }
            return 0;

        }
        private void LogUpdatingProgress(int logId, string msg)
        {
            LogHelper.Info(msg);
            HttpWebRequestHelper.Post(deployServiceHost + "/PackageReceive/UpdateReceiveInfo",
                new Dictionary<string, object>() { { "logId", logId }, { "msg", msg } });
        }
        private void LogFinishingUpdating(int logId, string error = "")
        {
            LogHelper.Info(string.Format("更新站点{0}。", string.IsNullOrWhiteSpace(error) ? "成功" : "失败") + (string.IsNullOrWhiteSpace(error) ? "" : "失败原因：" + error));
            HttpWebRequestHelper.Post(deployServiceHost + "/PackageReceive/FinishReceiving",
                new Dictionary<string, object>() { { "logId", logId }, { "error", error } });
        }

        private string GetNewestPackageUUId()
        {
            try
            {
                string s = HttpWebRequestHelper.Get(deployServiceHost + "/Deploy/GetNewDeployedPackageUUId?verified=" + requireVerifiedForNewPackage);
                if (string.IsNullOrWhiteSpace(s))
                    return string.Empty;
                JObject o = JsonConvert.DeserializeObject<JObject>(s);
                if (o["status"].ToObject<int>() == 0)
                {
                    return o["result"]["uuid"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            return string.Empty;
        }
        private bool NeedToDownloadDeployedPackage()
        {
            try
            {
                string s = HttpWebRequestHelper.Get(deployServiceHost + "/PackageReceive/NeedToDownloadDeployedPackage?hostName=" + Dns.GetHostName());
                if (string.IsNullOrWhiteSpace(s))
                    return false;
                JObject o = JsonConvert.DeserializeObject<JObject>(s);
                if (o["status"].ToObject<int>() == 0)
                {
                    return o["needToDownload"].ToObject<bool>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
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

        private bool IsWebsiteHealthy(int recordId)
        {
            for (int i = 0; i < 5; i++)
            {
                LogUpdatingProgress(recordId, DateTime.Now.ToLocalTime() + string.Format(" 第{0}次检查站点是否正常。", i + 1));
                try
                {
                    string s = HttpWebRequestHelper.Get(websiteHealthMonitorURL);
                    if ("Status OK".Equals(s))
                        return true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
                LogUpdatingProgress(recordId, DateTime.Now.ToLocalTime() + string.Format(" 站点访问异常，3秒后重试。", i + 1));
                Thread.Sleep(3000);
            }
            return false;

        }

        private string GetNewBackupPackagePath()
        {
            string[] dirList = Directory.GetDirectories(websiteDirectoryBackup);
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
