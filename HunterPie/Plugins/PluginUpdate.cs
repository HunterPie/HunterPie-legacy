using System;
using Debugger = HunterPie.Logger.Debugger;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace HunterPie.Plugins
{
    internal class PluginUpdate
    {
        public static bool PluginSupportsUpdate(PluginInformation pluginInformation)
        {
            return !string.IsNullOrEmpty(pluginInformation.Update.UpdateUrl);
        }

        public static async Task<UpdateResult> UpdateAllFiles(PluginInformation pInformation, string modPath)
        {
            string onlineSerializedInformation = await ReadOnlineModuleJson(pInformation.Update.UpdateUrl);

            if (onlineSerializedInformation is null)
            {
                //Debugger.Error($"Failed to update plugin: {pInformation.Name}!");
                return UpdateResult.Failed;
            }

            PluginInformation onlineInformation = JsonConvert.DeserializeObject<PluginInformation>(onlineSerializedInformation);

            if (!(Hunterpie.AssemblyVersion >= Hunterpie.ParseVersion(onlineInformation.Update.MinimumVersion)))
            {
                Debugger.Warn($"Newest version of {pInformation.Name} requires HunterPie v{onlineInformation.Update.MinimumVersion}!");
                return UpdateResult.Skipped;
            }

            UpdateResult result = UpdateResult.UpToDate;

            foreach (string filePath in onlineInformation.Update.FileHashes.Keys)
            {
                string onlineHash = onlineInformation.Update.FileHashes[filePath];

                if (onlineHash.ToLower() == "installonly" && File.Exists(Path.Combine(modPath, filePath)))
                {
                    continue;
                }

                if (pInformation.Update.FileHashes.ContainsKey(filePath))
                {
                    string localHash = pInformation.Update.FileHashes[filePath];

                    if (onlineHash.ToLower() != localHash.ToLower() || !File.Exists(Path.Combine(modPath, filePath)))
                    {
                        string updateurl = $"{pInformation.Update.UpdateUrl}/{filePath}";
                        string outputPath = Path.Combine(modPath, filePath);

                        if (!(await DownloadFileAsync(updateurl, outputPath, filePath)))
                        {
                            return UpdateResult.Failed;
                        }

                        result = UpdateResult.Updated;
                    }
                }
                else
                {
                    string updateurl = $"{pInformation.Update.UpdateUrl}/{filePath}";
                    string outputPath = Path.Combine(modPath, filePath);
                    if (!(await DownloadFileAsync(updateurl, outputPath, filePath)))
                    {
                        return UpdateResult.Failed;
                    }
                    result = UpdateResult.Updated;
                }
            }

            return await DownloadFileAsync($"{pInformation.Update.UpdateUrl}/module.json", Path.Combine(modPath, "module.json"), "module.json")
                ? result
                : UpdateResult.Failed;
        }

        public static async Task<bool> DownloadFileAsync(string URL, string output, string relFilepath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] data = await client.GetByteArrayAsync(new Uri(URL));

                    if (Path.GetDirectoryName(relFilepath).Length > 0)
                    {
                        string dirs = Path.GetDirectoryName(output);

                        if (!Directory.Exists(dirs))
                        {
                            Directory.CreateDirectory(dirs);
                        }

                    }
                    File.WriteAllBytes(output, data);
                    return true;
                }
            }
            catch (Exception err)
            {
                Debugger.Error(err);
                return false;
            }
        }

        internal static async Task<string> ReadOnlineModuleJson(string URL)
        {
            string url = URL;
            if (!URL.EndsWith("module.json"))
            {
                url = $"{URL}/module.json";
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync(new Uri(url));
                }
            }
            catch (Exception err)
            {
                Debugger.Error(err);
                return null;
            }
        }
    }
}
