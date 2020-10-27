using Microsoft.VisualStudio.TestTools.UnitTesting;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace Cn.Abp.Cli.Core.Tests
{
    [DependsOn(
        typeof(AbpTestBaseModule),
        typeof(AbpCliCoreModule))]
    public class AbpCliTestModule : AbpModule
    {


    }
}
