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
        public static void Copy(string solutionDirectory, string targetDirectory, string exclude = "")
        {
            string arguments = string.Format(" \"{0}\" \"{1}\" /i/e/v/Y", solutionDirectory, targetDirectory);
            if (File.Exists(exclude))
                arguments += string.Format(" /EXCLUDE:{0}", exclude);
            DosCommandHelper.Execute("xcopy", arguments);
        }
    }
}
