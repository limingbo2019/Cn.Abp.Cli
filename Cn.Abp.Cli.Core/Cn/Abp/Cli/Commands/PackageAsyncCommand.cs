using Cn.Abp.Cli.Args;
using Cn.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Volo.Abp.DependencyInjection;

namespace Cn.Abp.Cli.Core.Cn.Abp.Cli.Commands
{
    /// <summary>
    /// nugetpackage同步命令
    /// 1,在web上下载需要的包
    /// 2，把包传到nuget服务端里面
    /// </summary>
    public class PackageAsyncCommand : IConsoleCommand, ITransientDependency
    {
        public ILogger<PackageAsyncCommand> Logger { get; set; }
        public PackageAsyncCommand()
        {
            Logger = NullLogger<PackageAsyncCommand>.Instance;
        }
        public Task ExcuteAsync(CommandLineArgs commandLineArgs)
        {
            string rootPath = commandLineArgs.Options.GetOrNull(Options.Project.Short, Options.Project.Long);
            rootPath = rootPath ?? Directory.GetCurrentDirectory();
            Logger.LogInformation($"查找文件夹[{rootPath}]");
            var packages = LoadProjectPackages(rootPath);
            Logger.LogInformation($"准备下载[{packages.Count}]个包。");
            string sourceUrl = commandLineArgs.Options.GetOrNull(Options.SouceUrl.Short, Options.SouceUrl.Long);
            sourceUrl = sourceUrl ?? CliUrls.NugetOrg;
            string saveFolder = commandLineArgs.Options.GetOrNull(Options.DownloadFolder.Short, Options.DownloadFolder.Long);
            saveFolder = saveFolder ?? Directory.GetCurrentDirectory();
            DownloadPackages(sourceUrl, saveFolder, packages);
            Logger.LogInformation("执行下载完毕。");
            string pushUrl = commandLineArgs.Options.GetOrNull(Options.UploadUrl.Short, Options.UploadUrl.Long);
            string pushKey = commandLineArgs.Options.GetOrNull(Options.UploadKey.Short, Options.UploadKey.Long);
            if (pushUrl.IsNullOrWhiteSpace() || pushKey.IsNullOrWhiteSpace())
            {
                Logger.LogInformation($"上传url和key必须填写才能继续上传包url[{pushUrl}]key[{pushKey}]");
                return Task.CompletedTask;
            }
            ExcutePushCommand(packages, pushUrl, pushKey);
            Logger.LogInformation("执行上传包完毕。");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 使用控制台把包推送到nugetserver
        /// </summary>
        /// <param name="packages"></param>
        private void ExcutePushCommand(List<NugetPackage> packages, string pushUrl, string pushKey)
        {
            foreach (var p in packages)
            {
                PushCommand(p.SaveFullPath, pushUrl, pushKey);
            }
        }
        /// <summary>
        /// 简短提示
        /// </summary>
        /// <returns></returns>
        public string GetShortDescription()
        {
            return "同步项目所需要的nuget包供离线使用";
        }
        /// <summary>
        /// 用例
        /// </summary>
        /// <returns></returns>
        public string GetUsageInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("");
            sb.AppendLine("用例:");
            sb.AppendLine("");
            sb.AppendLine("  abp package-async <projectFolder> [options]");
            sb.AppendLine("");
            sb.AppendLine("选项（Options）:");
            sb.AppendLine("");
            sb.AppendLine("-s|--sourceUrl <source-url>               (默认: nuget.org)");
            sb.AppendLine("-d|--downloadFolder <download-folder>     (默认下载到:当前目录)");
            sb.AppendLine("-k|--uploadKey <key>                      (默认上传key:无)");
            sb.AppendLine("-u|--uploadUrl <upload-url>               (默认上传至:无)");
            sb.AppendLine("");
            sb.AppendLine("示例:");
            sb.AppendLine("");
            sb.AppendLine("  abp package-async --sourceUrl https://www.nuget.org/api/v2/package/");
            sb.AppendLine("");
            sb.AppendLine("查阅文档获取更详细信息: http://url");

            return sb.ToString();
        }

        /// <summary>
        /// 上传到指定nugetserver种
        /// </summary>
        /// <param name="packageFullPath"></param>
        /// <param name="pushUrl"></param>
        /// <param name="pushKey"></param>
        protected void PushCommand(string packageFullPath, string pushUrl, string pushKey)
        {
            string pushArguments = $" nuget push {packageFullPath} -k {pushKey} -s {pushUrl}";
            var processStartInfo = new System.Diagnostics.ProcessStartInfo("dotnet", pushArguments);
            System.Diagnostics.Process.Start(processStartInfo)?.WaitForExit();
        }
        /// <summary>
        /// 读取项目配置的包
        /// </summary>
        /// <param name="path">项目文件所在文件夹</param>
        /// <returns></returns>
        protected List<NugetPackage> LoadProjectPackages(string path)
        {
            string searchOption = "*.csproj";
            var files = Directory.GetFiles(path, searchOption, SearchOption.AllDirectories);
            if (files.Length <= 0)
            {
                throw new FileNotFoundException($"路径[{path}]匹配字符[{searchOption}]找不到文件！");
            }
            List<NugetPackage> lstPackage = new List<NugetPackage>();
            XmlDocument doc = new XmlDocument();
            foreach (var f in files)
            {
                doc.LoadXml(File.ReadAllText(f));
                //解析得到包    
                var elementPackages = doc.GetElementsByTagName("PackageReference");
                foreach (XmlElement e in elementPackages)
                {
                    NugetPackage package = new NugetPackage();
                    package.Name = e.GetAttribute("Include");
                    package.Version = e.GetAttribute("Version");
                    lstPackage.Add(package);
                }
            }

            return lstPackage;
        }

        protected void DownloadPackages(string sourceApiUrl, string saveFolder, List<NugetPackage> packages)
        {
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            saveFolder = saveFolder.EnsureEndsWith('\\');
            Logger.LogInformation($"文件将下载到[{saveFolder}]目录下。");
            WebClient client = new WebClient();
            foreach (var p in packages)
            {
                try
                {
                    p.SaveFullPath = saveFolder + p.FileName;
                    if (File.Exists(p.SaveFullPath))
                    {
                        Logger.LogInformation($"包[{p.SaveFullPath}]已经存在，将跳过。");
                        continue;
                    }
                    p.SetDownloadPath(sourceApiUrl);
                    Logger.LogInformation($"将下载[{p.DownloadFullPath}]");
                    client.DownloadFile(p.DownloadFullPath, p.SaveFullPath);
                    Logger.LogInformation($"下载完成[{p.FileName}]");
                }
                catch (Exception er)
                {
                    Logger.LogInformation("下载失败。", er);
                }
            }
        }

        /// <summary>
        /// 填写本命令的选项
        /// </summary>
        public static class Options
        {
            /// <summary>
            /// 源url
            /// </summary>
            public static class SouceUrl
            {
                public const string Short = "s";
                public const string Long = "sourceUrl";
            }
            /// <summary>
            /// 项目根目录
            /// </summary>
            public static class Project
            {
                public const string Short = "p";
                public const string Long = "projectFolder";
            }
            /// <summary>
            /// 下载至目录
            /// </summary>
            public static class DownloadFolder
            {
                public const string Short = "d";
                public const string Long = "downloadFolder";
            }
            /// <summary>
            /// 上传至的url
            /// </summary>
            public static class UploadUrl
            {
                public const string Short = "u";
                public const string Long = "uploadUrl";
            }
            public static class UploadKey
            {
                public const string Short = "k";
                public const string Long = "key";
            }
        }
    }

    /// <summary>
    /// 包信息封装
    /// </summary>
    public class NugetPackage
    {
        //https://www.nuget.org/api/v2/package/Abp/5.14.0
        /// <summary>
        /// 包名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 下载路径
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public void SetDownloadPath(string source)
        {
            source = source.EnsureEndsWith('/');
            string fullUrl = string.Concat(source, Name, "/", Version);
            this.DownloadFullPath = fullUrl;
        }
        public string DownloadFullPath { get; protected set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get { return string.Concat(Name, ".", Version, ".nupkg"); } }
        /// <summary>
        /// 保存完整路径
        /// </summary>
        public string SaveFullPath { get; set; }
    }
}
