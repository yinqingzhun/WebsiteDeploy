﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

namespace WebDeploy.Utils
{
    public static class IISHelper
    {
        public static bool StartWebSitePath(string websiteName)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("start site \"{0}/\"", websiteName);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return s.Contains(string.Format("“{0}”已成功启动", websiteName));
        }
        public static bool StopWebSitePath(string websiteName)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("stop site \"{0}/\"", websiteName);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return s.Contains(string.Format("“{0}”已成功停止", websiteName));
        }
        public static bool SetWebSitePath(string websiteName, string websitePath)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("set app \"{0}/\" -[path='/'].physicalPath:\"{1}\"", websiteName, websitePath);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return s.Contains(string.Format("APP 对象“{0}/”已更改", websiteName));
        }

        public static bool CreateAppPool(string appPoolName)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("add apppool /name:\"{0}\"  /managedRuntimeVersion:\"v4.0\" /autoStart:\"true\" /managedPipelineMode:\"Integrated\" ", appPoolName);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return s.Contains(string.Format("已添加 APPPOOL 对象“{0}”", appPoolName));
        }

        public static bool CreateWebsite(string websiteName, string websitePath, int port, string appPoolName = "")
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("add site /name:\"{0}\" /physicalPath:\"{1}\" /bindings:http/*:{2}: ", websiteName, websitePath, port);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return s.Contains(string.Format("已添加 SITE 对象“{0}”", websiteName));
        }

        public static bool SetAppPoolForWebsite(string websiteName, string appPoolName)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("set site \"{0}\" -[path='/'].applicationPool:\"{1}\"  ", websiteName, appPoolName);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return s.Contains(string.Format("SITE 对象“{0}”已更改", websiteName));
        }

        public static bool ExistWebsite(string websiteName)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("list site \"{0}\"", websiteName);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return !string.IsNullOrWhiteSpace(s);
        }


        public static bool ExistAppPool(string appPoolName)
        {
            const string cmdName = @"C:\Windows\System32\inetsrv\appcmd.exe";
            string arguments = string.Format("list app \"{0}\"", appPoolName);
            string s = DosCommandHelper.Execute(cmdName, arguments);
            LogHelper.Info(string.Format("执行命令 {0} {1}。执行的结果为：{2}", cmdName, arguments, s));
            return !string.IsNullOrWhiteSpace(s);
        }
    }


    //public class Cmd
    //{
    //    /// <summary>  
    //    /// 是否终止调用CMD命令执行  
    //    /// </summary>  
    //    private bool invokeCmdKilled = true;
    //    /// <summary>  
    //    /// 获取或设置是否终止调用CMD命令执行  
    //    /// </summary>  
    //    public bool InvokeCmdKilled
    //    {
    //        get { return invokeCmdKilled; }
    //        set
    //        {
    //            invokeCmdKilled = value;
    //            if (invokeCmdKilled)
    //            {
    //                if (p != null && !p.HasExited)
    //                {
    //                    killProcess(p.Id);
    //                }
    //            }
    //        }
    //    }
    //    private Process p;
    //    private Action<string> RefreshResult;

    //    /// <summary>  
    //    /// 调用CMD命令，执行指定的命令，并返回命令执行返回结果字符串  
    //    /// </summary>  
    //    /// <param name="cmdArgs">命令行</param>  
    //    /// <param name="RefreshResult">刷新返回结果字符串的事件</param>  
    //    /// <returns></returns>  
    //    public void InvokeCmd(string cmdArgs, Action<string> pRefreshResult = null)
    //    {
    //        InvokeCmdKilled = false;
    //        RefreshResult = pRefreshResult;
    //        if (p != null)
    //        {
    //            p.Close();
    //            p = null;
    //        }
    //        p = new Process();
    //        p.StartInfo.FileName = "cmd.exe";
    //        p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
    //        p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
    //        p.StartInfo.UseShellExecute = false;
    //        p.StartInfo.RedirectStandardInput = true;
    //        p.StartInfo.RedirectStandardOutput = true;
    //        p.StartInfo.RedirectStandardError = true;
    //        p.StartInfo.CreateNoWindow = true;
    //        p.Start();
    //        p.BeginErrorReadLine();
    //        p.BeginOutputReadLine();

    //        string[] cmds = cmdArgs.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    //        foreach (var v in cmds)
    //        {
    //            Thread.Sleep(200);
    //            p.StandardInput.WriteLine(v);
    //        }
    //        p.WaitForExit();
    //        p.Dispose();
    //        p.Close();
    //        p = null;
    //        InvokeCmdKilled = true;
    //    }
    //    /// <summary>  
    //    /// 输入交互式命令  
    //    /// </summary>  
    //    /// <param name="cmd"></param>  
    //    public void InputCmdLine(string cmd)
    //    {
    //        if (p == null) return;
    //        string[] cmds = cmd.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    //        foreach (var v in cmds)
    //        {
    //            Thread.Sleep(200);
    //            p.StandardInput.WriteLine(v);
    //        }
    //    }
    //    /// <summary>  
    //    /// 异步读取标准输出信息  
    //    /// </summary>  
    //    /// <param name="sender"></param>  
    //    /// <param name="e"></param>  
    //    void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
    //    {
    //        if (RefreshResult != null && e.Data != null)
    //            RefreshResult(e.Data + "\r\n");
    //    }
    //    /// <summary>  
    //    /// 异步读取错误消息  
    //    /// </summary>  
    //    /// <param name="sender"></param>  
    //    /// <param name="e"></param>  
    //    void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    //    {
    //        if (RefreshResult != null && e.Data != null)
    //        {
    //            RefreshResult(e.Data + "\r\n");
    //        }
    //    }
    //    /// <summary>  
    //    /// 关闭指定进程ID的进程以及子进程（关闭进程树）  
    //    /// </summary>  
    //    /// <param name="id"></param>  
    //    public void FindAndKillProcess(int id)
    //    {
    //        killProcess(id);
    //    }
    //    /// <summary>  
    //    /// 关闭指定进程名称的进程以及子进程（关闭进程树）  
    //    /// </summary>  
    //    /// <param name="name"></param>  
    //    public void FindAndKillProcess(string name)
    //    {
    //        foreach (Process clsProcess in Process.GetProcesses())
    //        {
    //            if ((clsProcess.ProcessName.StartsWith(name, StringComparison.CurrentCulture)) || (clsProcess.MainWindowTitle.StartsWith(name, StringComparison.CurrentCulture)))
    //                killProcess(clsProcess.Id);
    //        }
    //    }
    //    /// <summary>  
    //    /// 关闭进程树  
    //    /// </summary>  
    //    /// <param name="pid"></param>  
    //    /// <returns></returns>  
    //    private bool killProcess(int pid)
    //    {
    //        Process[] procs = Process.GetProcesses();
    //        for (int i = 0; i < procs.Length; i++)
    //        {
    //            if (getParentProcess(procs[i].Id) == pid)
    //                killProcess(procs[i].Id);
    //        }

    //        try
    //        {
    //            Process myProc = Process.GetProcessById(pid);
    //            myProc.Kill();
    //        }
    //        //进程已经退出  
    //        catch (ArgumentException)
    //        {
    //            ;
    //        }
    //        return true;
    //    }
    //    /// <summary>  
    //    /// 获取父进程ID  
    //    /// </summary>  
    //    /// <param name="Id"></param>  
    //    /// <returns></returns>  
    //    private int getParentProcess(int Id)
    //    {
    //        int parentPid = 0;
    //        using (ManagementObject mo = new ManagementObject("win32_process.handle='" + Id.ToString(CultureInfo.InvariantCulture) + "'"))
    //        {
    //            try
    //            {
    //                mo.Get();
    //            }
    //            catch (ManagementException)
    //            {
    //                return -1;
    //            }
    //            parentPid = Convert.ToInt32(mo["ParentProcessId"], CultureInfo.InvariantCulture);
    //        }
    //        return parentPid;
    //    }

    //}
}
