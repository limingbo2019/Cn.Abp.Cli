using Volo.Abp.Modularity;
using Volo.Abp.Autofac;

namespace Cn.Abp.Cli
{
    [DependsOn(
        typeof(AbpCliCoreModule),
        typeof(AbpAutofacModule))]
    public class CnAbpCliModule : AbpModule
    {

    }
}