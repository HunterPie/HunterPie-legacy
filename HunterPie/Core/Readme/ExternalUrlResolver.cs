using System.Collections.Generic;
using HunterPie.Utils;

namespace HunterPie.Core.Readme
{
    public class ExternalUrlResolver : IUrlResolver
    {
        private readonly string root;
        public readonly Dictionary<string, string> Links = new();

        public ExternalUrlResolver(string root)
        {
            this.root = root;
        }

        public string Resolve(string url)
        {
            if (UriUtilities.IsAbsoluteUrl(url))
            {
                Links[url] = url;
                return url;
            }

            var newUrl = UriUtilities.UrlCombine(root, url);
            Links[url] = newUrl;

            return newUrl;
        }
    }
}
