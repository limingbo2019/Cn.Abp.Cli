using Cn.Abp.Cli.Core.Cn.Abp.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace Cn.Abp.Cli.Args
{
    public class CommandLineArgumentParser : ICommandLineArgumentParser, ITransientDependency
    {
        public CommandLineArgs Parser(string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                return CommandLineArgs.Empty();
            }
            var argumentList = args.ToList();//使用list操作
            //命令
            var command = argumentList[0];
            argumentList.RemoveAt(0);//从队列中移除已经赋值的项

            if (!argumentList.Any())//如果命令后面没有任何参数，则不再继续
            {
                return new CommandLineArgs(command);
            }

            //目的，目标
            var target = argumentList[0];

            if (target.StartsWith("-"))
            {
                target = null;//如果是选项，则不移除,只是把target设置为null
            }
            else
            {
                argumentList.RemoveAt(0);//如果不是选项，则移除，作为target来做
            }

            if (!argumentList.Any())//如果已经没有，则说明没有选项
            {
                return new CommandLineArgs(command, target);
            }
            //选项
            var commandLineArgs = new CommandLineArgs(command, target);
            while (argumentList.Any())//遍历剩下的选项和参数,添加到类里面
            {
                var optionName = ParseOptionName(argumentList[0]);
                argumentList.RemoveAt(0);
                if (!argumentList.Any())
                {
                    commandLineArgs.Options[optionName] = null;//如果选项没有参数，则设置为null
                    break;
                }

                if (IsOptionName(argumentList[0]))//选项之后应该跟着参数，如果选项之后还跟着选项，则不应该，所以判断是否是选项
                {
                    commandLineArgs.Options[argumentList[0]] = null;
                    continue;
                }
                commandLineArgs.Options[optionName] = argumentList[0];//把参数放到选项里面去
                argumentList.RemoveAt(0);
            }
            return commandLineArgs;
        }
        /// <summary>
        /// 判断是否是选项名称
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static bool IsOptionName(string argument)
        {
            return argument.StartsWith("-") || argument.StartsWith("--");
        }
        /// <summary>
        /// 粘贴选项名称
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static string ParseOptionName(string argument)//为什么用静态方法呢？
        {
            //“--”之后一般用单词，“-”一般是用单词的第一个字母作为缩写

            if (argument.StartsWith("--"))
            {
                if (argument.Length <= 2)
                {
                    throw new ArgumentException("选项名字应该用--来做前缀，例如--[选项]");
                }
                return argument.RemovePreFix("--");//移除“--”后就是选项名称了
            }
            if (argument.StartsWith("-"))
            {
                if (argument.Length <= 1)
                {
                    throw new ArgumentException("选项缩写应该用-来做前缀，例如-[选项缩写]");
                }
                return argument.RemovePreFix("-");
            }
            throw new ArgumentException("选项应该前缀-或者--");
        }
    }
}
