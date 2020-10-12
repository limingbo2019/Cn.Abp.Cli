using Cn.Abp.Cli.Args;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cn.Abp.Cli.Commands
{
    public class GenerateProxyCommand : IConsoleCommand, ITransientDependency
    {
        public Task ExcuteAsync(CommandLineArgs commandLineArgs)
        {
            Console.WriteLine("你没有实现咯");
            return Task.CompletedTask;
        }

        public string GetShortDescription()
        {
            return "生成TS服务类以及Dtos文件";
        }

        public string GetUsageInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("");
            sb.AppendLine("用例:");
            sb.AppendLine("");
            sb.AppendLine("  abp generate-proxy [options]");
            sb.AppendLine("");
            sb.AppendLine("选项（Options）:");
            sb.AppendLine("");
            sb.AppendLine("-a|--apiUrl <api-url>               (默认: environment.ts>apis>default>url)");
            sb.AppendLine("-u|--ui <ui-framework>               (默认UI库: angular)");
            sb.AppendLine("-m|--module <module>               (默认模块: app)");
            sb.AppendLine("");
            sb.AppendLine("示例:");
            sb.AppendLine("");
            sb.AppendLine("  abp generate-proxy --apiUrl https://www.volosoft.com");
            sb.AppendLine("");
            sb.AppendLine("查阅文档获取更详细信息: https://docs.abp.io/en/abp/latest/CLI");

            return sb.ToString();
        }
    }
}
