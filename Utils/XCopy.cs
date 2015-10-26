using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebDeploy.Utils
{
    public class XCopy
    {
        /// <summary>
        /// 复制文件到指定目录
        /// </summary>
        /// <param name="srcDir">原文件目录</param>
        /// <param name="dstDir">存放目录</param>
        /// <returns>true</returns>
        public bool Copy(String srcDir, String dstDir)
        {
            if (!File.Exists(srcDir))
                return false;

            if (!File.Exists(dstDir))
                File.Create(dstDir);

            string cmd = string.Format("xcopy {0} {1} /i/e/v/Y/D", srcDir, dstDir);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string sOutput = p.StandardOutput.ReadToEnd();
            Console.WriteLine(sOutput);
            return true;
        }
    }
}
