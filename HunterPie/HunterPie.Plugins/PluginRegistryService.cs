using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using HunterPie.Core;
using HunterPie.Core.Enums;
using Newtonsoft.Json;

namespace HunterPie.Plugins
{
    public class PluginRegistryService
    {
        private readonly HttpClient client;

        private static readonly Lazy<PluginRegistryService> lazyInstance =
            new Lazy<PluginRegistryService>(() => new PluginRegistryService());

        public static PluginRegistryService Instance => lazyInstance.Value;

        private PluginRegistryService()
        {
            client = new HttpClient();
        }

        public async Task<List<PluginRegistryEntry>> GetAll()
        {
            var url = BuildUrl(UserSettings.PlayerConfig.HunterPie.PluginRegistryUrl, "plugins");
            if (await ShouldUseProxy())
            {
                url += "?proxy=true";
            }

            var rsp = await client.GetAsync(url);
            var str = await rsp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PluginRegistryEntry>>(str);
        }

        public async Task<int> ReportInstall(string pluginName)
        {
            var url = BuildUrl(UserSettings.PlayerConfig.HunterPie.PluginRegistryUrl, "plugin", Uri.EscapeUriString(pluginName), "install");
            var rsp = await client.PostAsync(url, new StringContent(""));
            rsp.EnsureSuccessStatusCode();
            return int.Parse(await rsp.Content.ReadAsStringAsync());
        }

        public async Task<string> GetModuleUpdateUrl(PluginInformation pInformation)
        {
            if (await ShouldUseProxy())
            {
                return BuildUrl(UserSettings.PlayerConfig.HunterPie.PluginRegistryUrl, "plugin", Uri.EscapeUriString(pInformation.Name), "module");
            }

            return pInformation.Update.UpdateUrl;
        }

        private string BuildUrl(params string[] parts) => string.Join("/", parts.Select(p => p.TrimEnd('/')));

        private async Task<bool> ShouldUseProxy()
        {
            var mode = UserSettings.PlayerConfig.HunterPie.PluginProxyMode;
            if (mode == PluginProxyMode.Enabled)
            {
                return true;
            }

            if (mode == PluginProxyMode.Disabled)
            {
                return false;
            }

            var registryAccessible = await PingService(UserSettings.PlayerConfig.HunterPie.PluginRegistryUrl);
            if (!registryAccessible)
            {
                return false;
            }

            var githubAccessible = await PingService("github.com");
            return githubAccessible;
        }

        private static async Task<bool> PingService(string address)
        {

            try
            {
                var pingSender = new Ping();
                for (var i = 0; i < 4; i++)
                {
                    var reply = await pingSender.SendPingAsync(address, 4000);
                    if (reply?.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public class PluginRegistryEntry
    {
        public string DisplayName { get; set; }
        public string InternalName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Readme { get; set; }
        public string Module { get; set; }
        public string MinVersion { get; set; }

        public string Author { get; set; }
        public string Version { get; set; }

        public int Downloads { get; set; }
    }
}
