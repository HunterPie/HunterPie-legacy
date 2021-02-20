using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using HunterPie.Logger;

namespace HunterPie.Core
{
    public class GStrings
    {
        public static XmlDocument Translations { get; private set; } = new XmlDocument();

        public static void InitStrings(string LangXml, Application App) => LoadTranslationXML(LangXml, App);

        private static void LoadTranslationXML(string LangXML, Application App)
        {
            if (LangXML is null)
            {
                LangXML = @"Languages\en-us.xml";
            }
            try
            {
                XmlDocument other = new XmlDocument();
                other.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LangXML));

                Translations.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Languages\en-us.xml"));

                // Merge other languages with the english localization
                XmlNodeList englishNodes = Translations.DocumentElement.SelectNodes("//*");
                foreach (XmlNode node in other.DocumentElement.SelectNodes("//*"))
                {
                    string id = node.Attributes["ID"]?.Value;
                    if (id is null)
                    {
                        continue;
                    }
                    XmlNode match = Translations.DocumentElement.SelectSingleNode($"//{node.ParentNode.Name}/*[@ID='{id}']");

                    if (match is null)
                    {
                        continue;
                    }

                    match.Attributes["Name"].Value = node.Attributes["Name"].Value;
                }
                Debugger.Warn($"Loaded {other.DocumentElement.Attributes["lang"]?.Value ?? "Unknown language"} game strings");
                other = null;
            } catch (Exception err)
            {
                Debugger.Error(err);
                Debugger.Error($"Failed to load {Path.GetFileName(LangXML)}");
                Translations.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Languages\en-us.xml"));
            }
            XmlDataProvider LocDataProvider = (XmlDataProvider)App.FindResource("Localization");
            LocDataProvider.Document = Translations;
            LocDataProvider.Refresh();
        }

        public static void LoadTranslationsFromStream(XmlDataProvider provider, Stream stream)
        {
            Translations.Load(stream);
            provider.Document = Translations;
            provider.Refresh();
        }

        /// <summary>
        /// Gets the mantle name by their id
        /// </summary>
        /// <param name="ID">Mantle id</param>
        /// <returns>Mantle name</returns>
        public static string GetMantleNameByID(int ID)
        {
            XmlNode Mantle = Translations.SelectSingleNode($"//Strings/Mantles/Mantle[@ID='{ID}']");
            return Mantle?.Attributes["Name"].Value;
        }

        /// <summary>
        /// Gets weapon name by their id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>Weapon name</returns>
        public static string GetWeaponNameByID(int ID)
        {
            XmlNode Weapon = Translations.SelectSingleNode($"//Strings/Weapons/Weapon[@ID='{ID}']");
            return Weapon?.Attributes["Name"].Value;
        }

        [Obsolete("This method is deprecated, use GMD.GetMonsterNameByEm(string monsterEm) instead.")]
        public static string GetMonsterNameByID(string ID)
        {
            XmlNode Monster = Translations.SelectSingleNode($"//Strings/Monsters/Monster[@ID='{ID}']");
            return Monster?.Attributes["Name"].Value;
        }

        /// <summary>
        /// Gets fertilizer name by id
        /// </summary>
        /// <param name="ID">Fertilizer id</param>
        /// <returns>Fertilizer name</returns>
        public static string GetFertilizerNameByID(int ID)
        {
            XmlNode Fertilizer = Translations.SelectSingleNode($"//Strings/Fertilizers/Fertilizer[@ID='{ID}']");
            return Fertilizer?.Attributes["Name"].Value;
        }

        /// <summary>
        /// Gets stage name by id
        /// </summary>
        /// <param name="ID">Stage id</param>
        /// <returns>Stage name</returns>
        public static string GetStageNameByID(int ID)
        {
            XmlNode Stage = Translations.SelectSingleNode($"//Strings/Stages/Stage[@ID='{ID}']");
            return Stage?.Attributes["Name"].Value;
        }

        /// <summary>
        /// Gets abnormality name by type, id and stack.<br/>
        /// <b>NOTE:</b> FOR HUNTING HORN BUFFS, USE <see cref="Native.GMD.GetMusicSkillNameById(int)"/> INSTEAD!
        /// </summary>
        /// <param name="Type">Abnormality type</param>
        /// <param name="ID">Abnormality id</param>
        /// <param name="Stack">Stack</param>
        /// <returns>Abnormality name</returns>
        public static string GetAbnormalityByID(string Type, int ID, int Stack)
        {
            XmlNode Abnormality = Translations.SelectSingleNode($"//Strings/Abnormalities/Abnormality[@ID='{Type}_{ID:000}_{Stack:00}']");
            if (Abnormality == null)
            {
                Abnormality = Translations.SelectSingleNode($"//Strings/Abnormalities/Abnormality[@ID='{Type}_{ID:000}_{0:00}']");
            }
            if (Abnormality == null)
            {
                Abnormality = Translations.SelectSingleNode($"//Strings/Abnormalities/Abnormality[@ID='UNKNOWN_ABNORMALITY']");
            }
            return Abnormality?.Attributes["Name"].Value.Replace("{AbnormalityID}", ID.ToString());
        }

        /// <summary>
        /// Gets monster part name by their stringified id
        /// </summary>
        /// <param name="PartStringID">Part id</param>
        /// <returns>Part name</returns>
        public static string GetMonsterPartByID(string PartStringID)
        {
            XmlNode PartName = Translations.SelectSingleNode($"//Strings/Parts/Part[@ID='{PartStringID}']");
            if (PartName == null) return "Missing translation";
            return PartName.Attributes["Name"].Value;
        }

        /// <summary>
        /// Gets monster ailment by id
        /// </summary>
        /// <param name="AilmentID">Ailment id</param>
        /// <returns>Ailment name</returns>
        public static string GetAilmentNameByID(string AilmentID)
        {
            XmlNode AilmentName = Translations.SelectSingleNode($"//Strings/Ailments/Ailment[@ID='{AilmentID}']");
            if (AilmentName == null) return Translations.SelectSingleNode($"//Strings/Ailments/Ailment[@ID='STATUS_UNKNOWN']")?.Attributes["Name"].Value + $" ({AilmentID})" ?? $"Unknown ({AilmentID})";
            return AilmentName.Attributes["Name"].Value;
        }

        /// <summary>
        /// Gets localization strings from //Strings/Client
        /// </summary>
        /// <param name="XPath">XPath</param>
        /// <returns>String</returns>
        public static string GetLocalizationByXPath(string XPath)
        {
            XmlNode LocString = Translations.SelectSingleNode($"//Strings/Client{XPath}");
            if (LocString == null) return XPath;
            return LocString.Attributes["Name"].Value;
        }

    }
}
