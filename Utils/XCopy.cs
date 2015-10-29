using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace WebDeploy.Utils
{
    public class XCopy
    {
        /// <summary>
        /// 复制文件到指定目录
        /// </summary>
        /// <param name="srcDir">原文件目录</param>
        /// <param name="dstDir">存放目录</param>
        /// <param name="exclude">排除的文件</param>
        /// <returns>true</returns>
        //public static bool Copy(String srcDir, String dstDir, string exclude = "")
        //{
        //    if (!File.Exists(srcDir))
        //        return false;

        //    if (!File.Exists(dstDir))
        //        File.Create(dstDir);

        //    string cmd = string.Format("xcopy {0} {1} /i/e/v/Y", srcDir, dstDir);
        //    if (File.Exists(exclude))
        //        cmd = string.Format("{0} /EXCLUDE:'{1}'", cmd, exclude);

        //    System.Diagnostics.Process p = new System.Diagnostics.Process();
        //    p.StartInfo.FileName = "cmd.exe";
        //    p.StartInfo.UseShellExecute = false;
        //    p.StartInfo.RedirectStandardInput = true;
        //    p.StartInfo.RedirectStandardOutput = true;
        //    p.StartInfo.CreateNoWindow = true;
        //    p.Start();
        //    string sOutput = p.StandardOutput.ReadToEnd();
        //    return true;
        //}

        public static void Copy(string SolutionDirectory, string TargetDirectory)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            //Give the name as Xcopy
            startInfo.FileName = "xcopy.exe";
            //make the window Hidden
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //Send the Source and destination as Arguments to the process
            startInfo.Arguments = "\"" + SolutionDirectory + "\"" + " " + "\"" + TargetDirectory + "\"" + @" /e /y /I";
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }
    }
}
