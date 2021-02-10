using System.Windows.Data;
using HunterPie.Core;
using HunterPie.Utils;

namespace HunterPie
{
    public class HunterPieXmlDataProvider : XmlDataProvider
    {
        public HunterPieXmlDataProvider()
        {
            // load embedded en-us localization if design time
            if (DesignUtils.IsInDesignMode && (Document == null || Document.ChildNodes.Count == 0))
            {
                using var res =
                    typeof(Hunterpie).Assembly.GetManifestResourceStream("HunterPie.Languages.en-us.xml");
                GStrings.LoadTranslationsFromStream(this, res);
            }
        }
    }
}
