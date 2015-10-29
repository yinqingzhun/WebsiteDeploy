using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace WebDeploy.Utils
{
    public class ZipHelper
    {
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourcePath">待压缩的目录</param>
        /// <param name="zipFilePath">待生成的压缩文件</param>
        public static void CreateZipFile(string sourcePath, string zipFilePath)
        {

            if (!Directory.Exists(sourcePath) && !File.Exists(sourcePath))
                throw new IOException("找不到目录或文件sourcePath");

            if (!zipFilePath.ToLower().EndsWith(".zip"))
            {
                zipFilePath += ".zip";
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
            {
                s.SetLevel(9); // 压缩级别 0-9
                //s.Password = "123"; //Zip压缩文件密码

                if (Directory.Exists(sourcePath))
                    AppendZipDirectory(s, null, sourcePath);
                else if (File.Exists(sourcePath))
                    AppendZipFile(s, null, sourcePath);

                s.Finish();
                s.Close();

            }



        }
        private static void AppendZipDirectory(ZipOutputStream stream, string parentPath, string dir)
        {
            //压缩目录下的文件
            Directory.GetFiles(dir).ToList().ForEach(p => AppendZipFile(stream, Path.Combine(parentPath ?? "", Path.GetFileName(dir)), p));
            //压缩目录下的目录
            Directory.GetDirectories(dir).ToList().ForEach(p =>
            {

                AppendZipDirectory(stream,
                    Path.Combine(parentPath ?? "", Path.GetFileName(dir)),
                    p);
            });
        }
        private static void AppendZipFile(ZipOutputStream stream, string parentDirName, string filePath)
        {
            if (stream == null)
                throw new ArgumentNullException();
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            byte[] buffer = new byte[4096]; //缓冲区大小
            ZipEntry entry = new ZipEntry(Path.Combine(parentDirName ?? "", Path.GetFileName(filePath)));
            entry.DateTime = DateTime.Now;
            stream.PutNextEntry(entry);
            using (FileStream fs = File.OpenRead(filePath))
            {
                int count = 0;
                while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, count);
                }

            }
        }
        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="zipFilePath">压缩文件</param>
        /// <param name="destinationPath">解压到的目录</param>
        public static void UnZipFile(string zipFilePath, string destinationPath)
        {

            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException();
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (directoryName.Length > 0 && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(Path.Combine(destinationPath, directoryName));
                    }

                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(Path.Combine(destinationPath, theEntry.Name)))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while ((size = s.Read(data, 0, data.Length)) > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }

                        }

                    }

                }

            }

        }
    }
}
