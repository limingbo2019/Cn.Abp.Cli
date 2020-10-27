using Cn.Abp.Cli.Core.Cn.Abp.Cli.Args;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cn.Abp.Cli.Args
{
    /// <summary>
    /// 程序.exe "命令" <目标> [选项]
    /// 例子
    /// abp add-package abp -p
    /// </summary>
    public class CommandLineArgs
    {
        /// <summary>
        /// 命令，跟着在程序后面
        /// </summary>
        [CanBeNull]
        public string Command { get; }
        /// <summary>
        /// 目标
        /// </summary>
        [CanBeNull]
        public string Target { get; }
        /// <summary>
        /// 选项
        /// </summary>
        public CommandLineOptions Options { get; internal set; }

        public CommandLineArgs([CanBeNull] string command = null, [CanBeNull] string target = null)
        {
            Command = command;
            Target = target;
            Options = new CommandLineOptions();
        }

        internal static CommandLineArgs Empty()
        {
            return new CommandLineArgs();
        }
    }
}
