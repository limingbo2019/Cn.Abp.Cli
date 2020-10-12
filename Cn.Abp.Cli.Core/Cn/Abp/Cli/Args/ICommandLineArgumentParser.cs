using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cn.Abp.Cli.Args
{
    /// <summary>
    /// 命令行一般按空格分割字符成字符串
    /// 该接口把字符串转换成有效的类
    /// </summary>
    public interface ICommandLineArgumentParser
    {
        CommandLineArgs Parser(string[] args);
    }
}
