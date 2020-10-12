using Cn.Abp.Cli.Core.Cn.Abp.Cli.Args;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cn.Abp.Cli.Args
{
    public class CommandLineArgs
    {
        [CanBeNull]
        public string Command { get;  }
        [CanBeNull]
        public string Target { get; }
        public CommandLineOptions Options { get; internal set; }

        public CommandLineArgs([CanBeNull]string command=null,[CanBeNull]string target=null)
        {
            this.Command = command;
            Target = target;
        }

        internal static CommandLineArgs Empty()
        {
            return new CommandLineArgs();
        }
    }
}
