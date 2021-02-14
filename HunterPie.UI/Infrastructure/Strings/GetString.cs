using System;
using System.ComponentModel;
using System.Windows.Markup;
using HunterPie.Core;

namespace HunterPie.Infrastructure.Strings
{
    [MarkupExtensionReturnType(typeof(string))]
    public class GetString : MarkupExtension
    {
        [DefaultValue("Console")]
        public string Section { get; set; }
        public string Key { get; set; }

        public GetString()
        {
        }

        public GetString(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return GStrings.GetLocalizationByXPath($"/{Section}/String[@ID='{Key}']");
        }
    }

    [MarkupExtensionReturnType(typeof(string))]
    public class Settings : MarkupExtension
    {
        public string Key { get; set; }

        public Settings()
        {
        }

        public Settings(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // //Strings/Client/Settings/String[@ID='{Key}']/@Name
            return GStrings.GetLocalizationByXPath($"/Settings/String[@ID='{Key}']");
        }
    }
}
