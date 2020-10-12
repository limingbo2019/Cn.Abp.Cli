using Cn.Abp.Cli.Args;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cn.Abp.Cli.Commands
{
    public interface ICommandSelector
    {
        Type Select(CommandLineArgs commandLineArgs);
    }
}
