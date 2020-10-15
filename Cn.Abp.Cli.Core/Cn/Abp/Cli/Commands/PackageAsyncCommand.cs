using Cn.Abp.Cli.Args;
using Cn.Abp.Cli.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cn.Abp.Cli.Core.Cn.Abp.Cli.Commands
{
    /// <summary>
    /// nugetpackage同步命令
    /// 1,在web上下载需要的包
    /// 2，把包传到nuget服务端里面
    /// </summary>
    public class PackageAsyncCommand : IConsoleCommand, ITransientDependency
    {
        public Task ExcuteAsync(CommandLineArgs commandLineArgs)
        {
            Console.WriteLine("准备下载,请先实现。");
            Console.WriteLine("第一步，读取项目*.csproj文件的包引用（xml格式）");
            Console.WriteLine("第二步,下载nupkg包");
            Console.WriteLine("第三步，上传至本地服务处");
            return Task.CompletedTask;
        }

        public string GetShortDescription()
        {
            return "同步项目所需要的nuget包供离线使用";
        }

        public string GetUsageInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("");
            sb.AppendLine("用例:");
            sb.AppendLine("");
            sb.AppendLine("  abp package-async [options]");
            sb.AppendLine("");
            sb.AppendLine("选项（Options）:");
            sb.AppendLine("");
            sb.AppendLine("-s|--sourceUrl <source-url>               (默认: nuget.org)");
            sb.AppendLine("-p|--projectFolder <project-folder>       (默认:当前目录)");
            sb.AppendLine("-d|--downloadFolder <download-folder>     (默认下载到:当前目录)");
            sb.AppendLine("-u|--uploadUrl <upload-url>               (默认上传至:无)");
            sb.AppendLine("");
            sb.AppendLine("示例:");
            sb.AppendLine("");
            sb.AppendLine("  abp package-async --sourceUrl https://www.nuget.org/api/v2/package/");
            sb.AppendLine("");
            sb.AppendLine("查阅文档获取更详细信息: http://url");

            return sb.ToString();
        }


        protected string GetPushCommand()
        {
            string test = "dotnet nuget push Cn.Abp.Cli.1.0.0.nupkg -k guang-zhou-nuget-server -s http://xxx.xxx.xxx.xxx:yyyy/nuget";
            return test;//推送格式
        }
    }
}
