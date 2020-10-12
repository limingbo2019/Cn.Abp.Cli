using System;
using System.Collections.Generic;
using System.Text;

namespace Cn.Abp.Cli.Core.Cn.Abp.Cli
{
    /// <summary>
    /// 用例错误
    /// 非致命错误
    /// </summary>
    public class CliUsageException : Exception
    {
        public CliUsageException(string message) : base(message)
        {

        }
        public CliUsageException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
