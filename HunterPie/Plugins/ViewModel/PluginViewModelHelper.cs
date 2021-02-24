using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.UI.Infrastructure;

namespace HunterPie.Plugins
{
    public static class PluginViewModelHelper
    {
        public static string FormatShortDate(DateTime? dateTime) => dateTime?.ToShortDateString();
        public static string FormatLongDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }

            var diffDate = DateTime.Now.Subtract(dateTime.Value);
            string diffStr;
            if (diffDate.TotalDays >= 1)
            {
                diffStr = GStrings
                    .GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_LAST_UPDATE_STRING_DAYS_PART']")
                    .Replace("{DaysAgo}", ((int)diffDate.TotalDays).ToString());
            }
            else
            {
                diffStr = GStrings
                    .GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_LAST_UPDATE_STRING_HOURS_PART']")
                    .Replace("{HoursAgo}", ((int)diffDate.TotalHours).ToString());
            }

            return GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_LAST_UPDATE_STRING']")
                .Replace("{UpdateTime}", dateTime.Value.ToLongDateString())
                .Replace("{TimeAgo}", diffStr);
        }

        public static bool TryParseGithubLink(string readme, out PluginActionViewModel action)
        {
            var regex = new Regex(@"https://raw.githubusercontent.com/([^/]+)/([^/]+)");
            if (readme != null)
            {
                var match = regex.Match(readme);
                if (match.Success)
                {
                    var url = $"https://github.com/{match.Groups[1]}/{match.Groups[2]}";
                    action = ParseLink(url);
                    return true;
                }
            }

            action = null;
            return false;
        }

        public static PluginActionViewModel ParseLink(string linkString)
        {
            var name = "";
            var link = "";
            ImageSource img = null;

            var parts = linkString.Split('|');

            switch (parts.Length)
            {
                // icon|name|link
                case 3:
                    name = GetLinkName(parts[0]);
                    img = (Application.Current.FindResource(parts[1]) as ImageSource)
                          ?? Application.Current.FindResource("ICON_LINK") as ImageSource;
                    link = parts[2];
                    break;

                // name|link
                case 2:
                    name = parts[0];
                    link = parts[1];
                    img = GetImageForAction(link) ?? Application.Current.FindResource("ICON_LINK") as ImageSource;
                    break;

                // link
                default:
                    name = GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_HOMEPAGE']");
                    link = parts[0];
                    img = GetImageForAction(link) ?? Application.Current.FindResource("ICON_LINK") as ImageSource;
                    break;
            }

            var command = new ArglessRelayCommand(() => Process.Start(link));
            return new PluginActionViewModel(name, img, command);
        }

        public static string GetLinkName(string name)
        {
            if (name == null) return "";
            switch (name.ToLowerInvariant())
            {
                case "home":
                case "homepage":
                    return GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_HOMEPAGE']");

                case "discord":
                    return GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_DISCORD_SETTINGS']");
            }

            return name;
        }

        public static ImageSource GetImageForAction(string link)
        {
            if (link.StartsWith("https://github.com") || link.StartsWith("https://raw.githubusercontent.com"))
            {
                return Application.Current.FindResource("ICON_GITHUB") as ImageSource;
            }

            if (link.StartsWith("https://discord.gg/"))
            {
                return Application.Current.FindResource("ICON_DISCORD") as ImageSource;
            }

            if (link.StartsWith("http:") || link.StartsWith("https:"))
            {
                return Application.Current.FindResource("ICON_LINK") as ImageSource;
            }

            // default
            return Application.Current.FindResource("ICON_LINK") as ImageSource;
        }

    }
}
