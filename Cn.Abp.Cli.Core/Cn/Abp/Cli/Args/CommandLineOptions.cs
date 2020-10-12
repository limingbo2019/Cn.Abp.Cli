using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;

namespace Cn.Abp.Cli.Core.Cn.Abp.Cli.Args
{
    public class CommandLineOptions : Dictionary<string, string>//该类作为一个字典，保存选项
    {
        public string GetOrNull([NotNull] string name, params string[] alternativeNames)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));//检查名字是否是空

            var value = this.GetOrDefault(name);//如果用名字获取不到值，则用alternative来获取值，如果都没有，则返回null
            if (!value.IsNullOrWhiteSpace())
            {
                return value;
            }

            if (!alternativeNames.IsNullOrEmpty())
            {
                foreach (var alternativeName in alternativeNames)
                {
                    value = this.GetOrDefault(alternativeName);
                    if (!value.IsNullOrWhiteSpace())
                    {
                        return value;
                    }
                }
            }
            return null;
        }
    }
}
