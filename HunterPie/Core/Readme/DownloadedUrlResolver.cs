using System;
using System.IO;
using HunterPie.Utils;
using Newtonsoft.Json;

namespace HunterPie.Core.Readme
{
    public class DownloadedUrlResolver : IUrlResolver
    {
        private readonly string cacheRoot;
        private readonly Lazy<ReadmeIndex> lazyIndex;

        private ReadmeIndex Index => lazyIndex.Value;

        public DownloadedUrlResolver(string modRoot)
        {
            this.cacheRoot = Path.Combine(modRoot, ".cache");
            this.lazyIndex = new Lazy<ReadmeIndex>(GetIndex);
        }

        public string Resolve(string url)
        {
            if (Index.Images.TryGetValue(url, out var name))
            {
                var filePath = Path.Combine(cacheRoot, name);
                if (File.Exists(filePath))
                {
                    return new Uri($"file://{filePath}").ToString();
                }
            }

            return ResolveExternal(url);
        }

        public string ResolveExternal(string url)
        {
            if (UriUtilities.IsAbsoluteUrl(url) || string.IsNullOrEmpty(Index.Source))
            {
                return url;
            }

            var baseUrl = UriUtilities.GetBasePath(Index.Source);
            return UriUtilities.UrlCombine(baseUrl, url);
        }

        private ReadmeIndex GetIndex()
        {
            var indexPath = Path.Combine(cacheRoot, "index.json");
            ReadmeIndex index = new();
            if (File.Exists(indexPath))
                index = JsonConvert.DeserializeObject<ReadmeIndex>(File.ReadAllText(indexPath));
            return index;
        }
    }
}
