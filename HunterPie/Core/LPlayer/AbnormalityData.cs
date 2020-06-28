using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HunterPie.Core.LPlayer;
using HunterPie.Logger;

namespace HunterPie.Core
{
    public class AbnormalityData
    {
        private static XmlDocument AbnormalitiesData;
        private static List<AbnormalityInfo> huntingHornAbnormalities;
        private static List<AbnormalityInfo> palicoAbnormalities;
        private static List<AbnormalityInfo> blightAbnormalities;
        private static List<AbnormalityInfo> miscAbnormalities;
        private static List<AbnormalityInfo> gearAbnormalities;

        public static IReadOnlyCollection<AbnormalityInfo> HuntingHornAbnormalities => huntingHornAbnormalities;
        public static IReadOnlyCollection<AbnormalityInfo> PalicoAbnormalities => palicoAbnormalities;
        public static IReadOnlyCollection<AbnormalityInfo> BlightAbnormalities => blightAbnormalities;
        public static IReadOnlyCollection<AbnormalityInfo> MiscAbnormalities => miscAbnormalities;
        public static IReadOnlyCollection<AbnormalityInfo> GearAbnormalities => gearAbnormalities;

        static public void LoadAbnormalityData()
        {
            AbnormalitiesData = new XmlDocument();
            AbnormalitiesData.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.Resources/Data/AbnormalityData.xml"));
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_ABNORMALITIES_DATA_LOAD']"));

            LoadHuntingHornAbnormalities();
            LoadPalicoAbnormalities();
            LoadBlightAbnormalities();
            LoadMiscAbnormalities();
            LoadGearAbnormalities();

            // Unload Abnormalities Data since we don't need it anymore
            AbnormalitiesData = null;
        }

        private static void LoadHuntingHornAbnormalities() => huntingHornAbnormalities = LoadAbnormalityType("HUNTINGHORN", "HH", "//Abnormalities/HUNTINGHORN_Abnormalities/Abnormality");

        private static void LoadPalicoAbnormalities() => palicoAbnormalities = LoadAbnormalityType("PALICO", "PAL", "//Abnormalities/PALICO_Abnormalities/Abnormality");

        private static void LoadBlightAbnormalities() => blightAbnormalities = LoadAbnormalityType("DEBUFF", "DE", "//Abnormalities/DEBUFF_Abnormalities/Abnormality");

        private static void LoadMiscAbnormalities() => miscAbnormalities = LoadAbnormalityType("MISC", "MISC", "//Abnormalities/MISC_Abnormalities/Abnormality");

        private static void LoadGearAbnormalities() => gearAbnormalities = LoadAbnormalityType("GEAR", "GEAR", "//Abnormalities/GEAR_Abnormalities/Abnormality");

        private static List<AbnormalityInfo> LoadAbnormalityType(string type, string idPrefix, string xmlSelector)
        {
            XmlNodeList nodes = AbnormalitiesData
                .SelectNodes(xmlSelector) ?? throw new Exception("Could not get XML Abnormality nodes");

            return nodes
                .Cast<XmlNode>()
                .Select(node => AbnormalityXmlNodeToInfo(node, type, idPrefix))
                .ToList();
        }

        private static AbnormalityInfo AbnormalityXmlNodeToInfo(XmlNode node, string type, string idPrefix)
        {
            if (node?.Attributes == null)
            {
                throw new ArgumentNullException(nameof(node), "XmlNode and its attributes cannot be null!");
            }

            AbnormalityInfo abnormality = new AbnormalityInfo
            {
                Id = int.Parse(node.Attributes["ID"].Value),
                Type = type,
                Offset = int.Parse(node.Attributes["Offset"].Value, System.Globalization.NumberStyles.HexNumber),
                IsDebuff = bool.Parse(node.Attributes["IsDebuff"]?.Value ?? "False"),
                IsGearBuff = bool.Parse(node.Attributes["IsGearBuff"]?.Value ?? "False"),
                IsInfinite = bool.Parse(node.Attributes["IsInfinite"]?.Value ?? "False"),
                IconName = node.Attributes["Icon"]?.Value,
                HasConditions = bool.Parse(node.Attributes["HasConditions"]?.Value ?? "False"),
                ConditionOffset = int.Parse(node.Attributes["ConditionOffset"]?.Value ?? "0"),
                Stack = int.Parse(node.Attributes["Stack"]?.Value ?? "0"),
                IsPercentageBuff = bool.Parse(node.Attributes["IsPercentageBuff"]?.Value ?? "False"),
                MaxTimer = float.Parse(node.Attributes["MaxTimer"]?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture)
            };

            abnormality.InternalId = $"{idPrefix}_{abnormality.Id}";
            if (string.IsNullOrEmpty(abnormality.IconName))
            {
                abnormality.IconName = "ICON_MISSING";
            }

            return abnormality;
        }
    }
}
