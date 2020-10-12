using Cn.Abp.Cli.Args;
using Cn.Abp.Cli.Commands;
using Cn.Abp.Cli.Core.Cn.Abp.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cn.Abp.Cli
{
    /// <summary>
    /// 用于传参cli，在执行命令之前，可以做一些事情。
    /// 比如，检查版本是否最新了，或者提醒一下版本。
    /// </summary>
    public class CliService : ITransientDependency//因为需要通过注入使用一些服务，所以要继承这个接口
    {
        public ILogger<CliService> Logger { get; set; }//日志组件由属性注入来使用
        protected ICommandLineArgumentParser _commandLineArgumentParser { get; }
        protected ICommandSelector _commandSelector { get; }
        protected IHybridServiceScopeFactory _serviceScopeFactory { get; }
        public CliService(ICommandLineArgumentParser commandLineArgumentParser, ICommandSelector commandSelector, IHybridServiceScopeFactory serviceScopeFactory)
        {
            _commandLineArgumentParser = commandLineArgumentParser;
            _commandSelector = commandSelector;
            _serviceScopeFactory = serviceScopeFactory;
            Logger = NullLogger<CliService>.Instance;//初始化时，是空的，如果不通过属性注入，则没有日志
        }
        public async Task RunAsync(string[] args)
        {
            Logger.LogInformation("开始执行命令了。。。");
            //把参数转成类，容易操作
            var commamdLineArgs = _commandLineArgumentParser.Parser(args);
            //提取命令，
            var commandType = _commandSelector.Select(commamdLineArgs);//如果转换成类后，没有命令，则默认执行帮助命令
            //根据命令的类型，定位到命令类，传入参数后，执行命令
            using (var scope = _serviceScopeFactory.CreateScope())//服务混合作用域，
            {
                var command = (IConsoleCommand)scope.ServiceProvider.GetRequiredService(commandType);//通过类型获取已经在容器的服务（这些服务在cli.core的模块启动里面注入了）
                try
                {
                    await command.ExcuteAsync(commamdLineArgs);//执行命令
                }
                catch (CliUsageException cliex)
                {
                    Logger.LogWarning(cliex.Message);//记录告警信息
                }
                catch (Exception er)
                {
                    Logger.LogException(er);//记录出错的日志
                }
            }
            Logger.LogInformation("命令执行完毕！");
        }
    }
}
