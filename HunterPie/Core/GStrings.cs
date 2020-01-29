using System.Xml;
using HunterPie.Logger;

namespace HunterPie.Core {
    class GStrings {
        static private XmlDocument Translations = new XmlDocument();

        public static void InitStrings() {
            LoadTranslationXML();
        }

        private static void LoadTranslationXML() {
            try {
                Translations.Load(@"translations\en-us.xml");
                Debugger.Log($"Loaded {Translations.DocumentElement.Attributes["lang"].Value} game strings");
            } catch {
                Debugger.Error("Failed to load en-us.xml");
            }
            
        }

        public static string GetMantleNameByID(int ID) {
            XmlNode Mantle = Translations.SelectSingleNode($"//Strings/Mantles/Mantle[@ID='{ID}']");
            if (Mantle.Attributes["Name"] != null) {
                return Mantle.Attributes["Name"].Value;
            } else {
                return null;
            }
        }

        public static string GetWeaponNameByID(int ID) {
            XmlNode Weapon = Translations.SelectSingleNode($"//Strings/Weapons/Weapon[@ID='{ID}']");
            if (Weapon.Attributes["Name"] != null) {
                return Weapon.Attributes["Name"].Value;
            } else {
                return null;
            }
        }

        public static string GetMonsterNameByID(string ID) {
            XmlNode Monster = Translations.SelectSingleNode($"//Strings/Monsters/Monster[@ID='{ID}']");
            if (Monster.Attributes["Name"] != null) {
                return Monster.Attributes["Name"].Value;
            } else {
                return null;
            }
        }

        public static string GetFertilizerNameByID(int ID) {
            XmlNode Fertilizer = Translations.SelectSingleNode($"//Strings/Fertilizers/Fertilizer[@ID='{ID}']");
            if (Fertilizer.Attributes["Name"] != null) {
                return Fertilizer.Attributes["Name"].Value;
            } else {
                return null;
            }
        }

        public static string GetStageNameByID(int ID) {
            XmlNode Stage = Translations.SelectSingleNode($"//Strings/Stages/Stage[@ID='{ID}']");
            if (Stage.Attributes["Name"] != null) {
                return Stage.Attributes["Name"].Value;
            } else {
                return null;
            }
        }

    }
}
