using Cn.Abp.Cli.Args;
using Cn.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Volo.Abp.DependencyInjection;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml.Linq;

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
            string saveFolder = commandLineArgs.Options.GetOrNull(Options.DownloadFolder.Short, Options.DownloadFolder.Long);
            saveFolder = saveFolder ?? Directory.GetCurrentDirectory();

            List<NugetPackage> packages;

            string uploadOnly = commandLineArgs.Options.GetOrNull(Options.UploadOnly.Short, Options.UploadOnly.Long);
            if (string.IsNullOrWhiteSpace(uploadOnly))//需要下载,通过加载项目的包列表下载
            {//TODO缺少依赖包的下载
                string rootPath = commandLineArgs.Options.GetOrNull(Options.Project.Short, Options.Project.Long);
                rootPath = rootPath ?? Directory.GetCurrentDirectory();
                Logger.LogInformation($"查找文件夹[{rootPath}]");
                packages = LoadProjectPackages(rootPath);
                string sourceUrlVersion = commandLineArgs.Options.GetOrNull(Options.SourceUrlVersion.Short, Options.SourceUrlVersion.Long);
                sourceUrlVersion = sourceUrlVersion ?? "v3";
                Logger.LogInformation($"准备下载[{packages.Count}]个包。");
                string sourceUrl = GetSourceUrl(sourceUrlVersion);
                DownloadPackages(sourceUrl, sourceUrlVersion, saveFolder, packages);
                Logger.LogInformation("执行下载完毕。");
            }
            else
            {
                packages = LoadDownloadFolderPackages(saveFolder);//只上传文件
                Logger.LogInformation($"准备直接上传[{packages.Count}]个包。");
            }

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

        private List<NugetPackage> CheckDependencies(string fileFullPath)
        {
            List<NugetPackage> dependenciesPackages = new List<NugetPackage>();
            if (!File.Exists(fileFullPath))
            {
                Logger.LogInformation($"查看包{fileFullPath}失败，文件不存在！");
                return dependenciesPackages;
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(fileFullPath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Name.EndsWith(".nuspec"))
                    {
                        var xDoc = XDocument.Load(s);
                        var xElements = xDoc.Root.Elements().FirstOrDefault().Elements();
                        //packageName = xElements.FirstOrDefault(p => p.Name.LocalName == "id").Value;
                        //packageVersion = xElements.FirstOrDefault(p => p.Name.LocalName == "version").Value;
                        var dep_root = xElements.FirstOrDefault(p => p.Name.LocalName == "dependencies");
                        if (dep_root != null)
                        {
                            var _dependencies = dep_root.Elements();

                            foreach (var _depackage in _dependencies.Elements())
                            {
                                string id = _depackage.Attribute("id").Value;
                                string version = _depackage.Attribute("version").Value.TrimStart('[').TrimEnd(',', ' ', ')');
                                //下载
                                Logger.LogInformation($"需要下载依赖包{id}版本{version}");
                                NugetPackage package = new NugetPackage();
                                package.Name = id;
                                package.Version = version;
                                dependenciesPackages.Add(package);
                            }
                        }
                        else
                        {
                            Logger.LogInformation($"无依赖包...");
                        }
                    }
                }

                // Close can be ommitted as the using statement will do it automatically
                // but leaving it here reminds you that is should be done.
                s.Close();
            }

            return dependenciesPackages;
        }

        /// <summary>
        /// 读取已经存在的包
        /// 例如NUGET的缓存
        /// Windows：%userprofile%\.nuget\packages
        /// Mac/Linux：~/.nuget/packages
        /// </summary>
        /// <param name="saveFolder">读取根文件夹</param>
        /// <returns></returns>
        private List<NugetPackage> LoadDownloadFolderPackages(string saveFolder)
        {
            List<NugetPackage> lstPackage = new List<NugetPackage>();
            if (!Directory.Exists(saveFolder))
            {
                Logger.LogInformation($"文件夹{saveFolder}不存在..请检查！");
                return lstPackage;
            }
            DirectoryInfo directory = new DirectoryInfo(saveFolder);
            var files = directory.GetFiles("*.nupkg", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                NugetPackage package = new NugetPackage();
                package.SaveFullPath = f.FullName;
                package.Name = f.Name;
                lstPackage.Add(package);
            }

            return lstPackage;
        }

        private string GetSourceUrl(string sourceUrlVersion)
        {
            if (sourceUrlVersion == "v3")
            {
                var request = WebRequest.Create(CliUrls.NugetOrg_V3);
                using (StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.UTF8))
                {
                    string json = streamReader.ReadToEnd();
                    json = json.Replace("@id", "id");
                    json = json.Replace("@type", "type");
                    json = json.Replace("@context", "context");
                    json = json.Replace("@vocab", "vocab");
                    var nugetV3 = JsonConvert.DeserializeObject<NugetV3>(json);
                    var packageBase = nugetV3.resources.Where(p => p.type == "PackageBaseAddress/" + nugetV3.version).FirstOrDefault();
                    return packageBase.id;
                }
            }
            else
            {
                return CliUrls.NugetOrg_V2;
            }
        }

        /// <summary>
        /// 使用控制台把包推送到nugetserver
        /// </summary>
        /// <param name="packages"></param>
        private void ExcutePushCommand(List<NugetPackage> packages, string pushUrl, string pushKey)
        {
            int countTotal = packages.Count;
            int cur = 0;
            foreach (var p in packages)
            {
                Logger.LogInformation($"[{cur}/{countTotal}]准备上传包{p.SaveFullPath}");
                PushCommand(p.SaveFullPath, pushUrl, pushKey);
                cur++;
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
            sb.AppendLine("-v|--sourceUrlVersion <source-url-version>               (默认: v3版本)");
            sb.AppendLine("-d|--downloadFolder <download-folder>                    (默认下载到:当前目录)");
            sb.AppendLine("-k|--uploadKey <key>                                     (默认上传key:无)");
            sb.AppendLine("-u|--uploadUrl <upload-url>                              (默认上传至:无)");
            sb.AppendLine("-o|--uploadOnly <upload-only>                            (默认:无，仅上传)");
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
            string pushArguments = $" nuget push {packageFullPath} -k {pushKey} -s {pushUrl} --skip-duplicate";// --skip-duplicate 存在则跳过
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
        /// <summary>
        /// 下载包
        /// </summary>
        /// <param name="sourceApiUrl"></param>
        /// <param name="sourceUrlVersion"></param>
        /// <param name="saveFolder"></param>
        /// <param name="packages"></param>
        protected void DownloadPackages(string sourceApiUrl, string sourceUrlVersion, string saveFolder, List<NugetPackage> packages)
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
                    p.SetDownloadPath(sourceApiUrl, sourceUrlVersion);
                    Logger.LogInformation($"将下载[{p.DownloadFullPath}]");
                    client.DownloadFile(p.DownloadFullPath, p.SaveFullPath);
                    Logger.LogInformation($"下载完成[{p.FileName}]");
                    DownloadPackages(sourceApiUrl, sourceUrlVersion, saveFolder, CheckDependencies(p.SaveFullPath));
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
            /// 包源版本
            /// </summary>
            public static class SourceUrlVersion
            {
                public const string Short = "v";
                public const string Long = "sourceUrlVersion";
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
            /// <summary>
            /// 上传使用的api
            /// </summary>
            public static class UploadKey
            {
                public const string Short = "k";
                public const string Long = "key";
            }
            /// <summary>
            /// 只上传，不下载，非空即为真
            /// </summary>
            public static class UploadOnly
            {
                public const string Short = "o";
                public const string Long = "uploadOnly";
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
        public void SetDownloadPath(string source, string sourceVersion = "v2")
        {
            if (sourceVersion == "v2")
            {
                source = source.EnsureEndsWith('/');
                string fullUrl = string.Concat(source, Name, "/", Version);
                this.DownloadFullPath = fullUrl;
            }
            else if (sourceVersion == "v3")
            {
                //https://api.nuget.org/v3-flatcontainer/{包名}/{版本号}/{包名}.{版本号}.nupkg
                source = source.EnsureEndsWith('/');
                string fullUrl = string.Concat(source, Name, "/", Version, "/", Name, ".", Version, ".nupkg").ToLower();
                this.DownloadFullPath = fullUrl;
            }
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


    #region nuget v3 json class
    public class ResourcesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string comment { get; set; }
    }

    public class context
    {
        /// <summary>
        /// 
        /// </summary>
        public string vocab { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string comment { get; set; }
    }

    public class NugetV3
    {
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ResourcesItem> resources { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public context context { get; set; }
    }
    #endregion
}
