using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HunterPie.Utils
{
    public static class UriUtilities
    {
        public static string GetBasePath(string path)
        {
            // returning path without last part:
            //  http://foo.bar/baz/module.json -> http://foo.bar/baz
            //  file://C:\\foo\bar\module.json -> file://C://foo/bar

            path = path.Replace('\\', '/');
            var match = Regex.Match(path, @"[\\\/]", RegexOptions.RightToLeft);
            if (!match.Success) return path;
            return path.Substring(0, match.Index);
        }

        public static string UrlCombine(params string[] urls)
        {
            var uri = string.Join("/", new[] {urls[0].TrimEnd('/')}
                .Concat(urls.Skip(1).Select(url => url.TrimStart('/')))
            );
            return new Uri(uri).ToString();
        }

        public static bool IsAbsoluteUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out Uri _);
    }
}
