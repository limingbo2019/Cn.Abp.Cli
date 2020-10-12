using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cn.Abp.Cli
{
    public class CliPaths
    {
        public static string TemplateCache => Path.Combine(CnAbpRootPath, "templates");//模板存放位置
        public static string Log => Path.Combine(CnAbpRootPath, "cli", "logs");//日志文件

        public static readonly string CnAbpRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cnabp");//用户工具条的存放位置？
    }
}
