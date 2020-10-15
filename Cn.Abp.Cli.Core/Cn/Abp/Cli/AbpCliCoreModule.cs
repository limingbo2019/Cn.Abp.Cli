using Cn.Abp.Cli.Commands;
using Cn.Abp.Cli.Core.Cn.Abp.Cli.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Modularity;

namespace Cn.Abp.Cli
{
    public class AbpCliCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//支持多语言

            Configure<AbpCliOptions>(options =>
            {
                //所有实现的命令都在这里写入命令+命令类型
                options.Commands["generate-proxy"] = typeof(GenerateProxyCommand);//生成angular的ts代理类Dtos
                options.Commands["package-async"] = typeof(PackageAsyncCommand);//下载包上传到本地nugetserver里面
            });
        }
    }
}
