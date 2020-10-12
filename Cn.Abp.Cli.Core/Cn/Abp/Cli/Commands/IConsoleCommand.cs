using Cn.Abp.Cli.Args;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cn.Abp.Cli.Commands
{
    /// <summary>
    /// 命令全部继承该接口
    /// 控制台命令
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="commandLineArgs">参数</param>
        /// <returns></returns>
        Task ExcuteAsync(CommandLineArgs commandLineArgs);
        /// <summary>
        /// 使用教程
        /// </summary>
        /// <returns></returns>
        string GetUsageInfo();
        /// <summary>
        /// 简短提示
        /// </summary>
        /// <returns></returns>
        string GetShortDescription();
    }
}
