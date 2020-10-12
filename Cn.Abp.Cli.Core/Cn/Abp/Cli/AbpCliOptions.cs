using System;
using System.Collections.Generic;

namespace Cn.Abp.Cli
{
    public class AbpCliOptions
    {
        public Dictionary<string, Type> Commands { get; }
        public bool CacheTemplates { get; set; } = true;
        public string ToolName { get; set; } = "CLI";
        public AbpCliOptions()
        {
            Commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);//在初始化的时候，通过该选项保存其字典？
        }
    }
}