using System;
using System.IO;
using Serilog;
using Volo.Abp;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Threading;
using System.Text.Encodings;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Cn.Abp.Cli
{
    class Program
    {
        static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册实例？
            Console.OutputEncoding = System.Text.Encoding.GetEncoding("GB2312");//修改输出的编码
            Console.WriteLine("欢迎来到学习（shangzhai)轮子的工具条。");
            //Console.WriteLine("English support？");
            //配置日志组件Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)//把微软的日志提升到告警才输出
                .MinimumLevel.Override("Volo.Abp", Serilog.Events.LogEventLevel.Warning)//把abp框架的日志提升到告警才输出
                .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)//对类库System.Net.Http.HttpClient的日志提升
#if DEBUG
                .MinimumLevel.Override("Cn.Abp.Cli", Serilog.Events.LogEventLevel.Debug)//调试模式下输出调试的信息
#else
                .MinimumLevel.Override("Cn.Abp.Cli", Serilog.Events.LogEventLevel.Information)//生产模式下输出信息
#endif
                .Enrich.FromLogContext()//这里是干嘛？
                .WriteTo.File(Path.Combine(CliPaths.Log, "cn-abp-cli-logs"))//写日志文件
                .WriteTo.Console()//打印到控制台
                .CreateLogger();//创建日志

            //然后通过abp框架，加载模块,引用volo.abp.autofac
            //args = new string[] { "package-async", "-p", "E:\\GDProjects\\GD.Portal.PC.GzMes\\gd.portal.pc.gzmes\\src" };
            using (var application = AbpApplicationFactory.Create<CnAbpCliModule>(
                options =>
                {
                    options.UseAutofac();//依赖注入
                    options.Services.AddLogging(c => c.AddSerilog());//微软对serilog的拓展，添加服务的方式添加
                }))
            {
                application.Initialize();//初始化
                AsyncHelper.RunSync(
                    () => application.ServiceProvider
                    .GetRequiredService<CliService>()
                    .RunAsync(args)
                    );//但是这里没有注入日志组件啊？
                application.Shutdown();//执行完命令后关闭所有依赖的模块
            }
        }
    }
}
