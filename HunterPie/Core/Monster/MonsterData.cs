using HunterPie.Properties;
using HunterPie.Logger;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core {
    class MonsterData {
        private static XmlDocument MonsterDataDocument;

        static public void LoadMonsterData() {
            MonsterDataDocument = new XmlDocument();
            MonsterDataDocument.LoadXml(Resources.MonsterData);
            Debugger.Warn("Loaded monster data");
        }

        static public void UnloadMonsterData() {
            MonsterDataDocument = null;
            Debugger.Warn("Unloaded monster data");
        }

        static public Dictionary<string, int> GetMonsterWeaknessById(string ID) {
            XmlNode Weaknesses = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{ID}']/Weaknesses");
            if (Weaknesses == null) return null;
            Dictionary<string, int> MonsterWeaknesses = new Dictionary<string, int>();
            foreach (XmlNode Weakness in Weaknesses) {
                MonsterWeaknesses.Add(Weakness.Attributes["ID"].Value, System.Convert.ToInt32(Weakness.Attributes["Stars"].Value));
            }
            return MonsterWeaknesses;
        }

        static public string GetMonsterCrownByMultiplier(string ID, float multiplier) {
            multiplier *= 100;
            XmlNode Crowns = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{ID}']/Crown");
            if (Crowns == null) return null;
            float Mini = float.Parse(Crowns.Attributes["Mini"].Value);
            float Silver = float.Parse(Crowns.Attributes["Silver"].Value);
            float Gold = float.Parse(Crowns.Attributes["Gold"].Value);
            if (multiplier >= Gold) return "CROWN_GOLD";
            if (multiplier >= Silver) return "CROWN_SILVER";
            if (multiplier <= Mini) return "CROWN_MINI";
            return null;
        }

        static public int GetMaxPartsByMonsterID(string ID) {
            XmlNode MonsterParts = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{ID}']/Parts");
            if (MonsterParts == null) return 0;
            int nParts = int.Parse(MonsterParts.Attributes["Max"].Value);
            return nParts;
        }

        static public int GetMaxRemovablePartsByMonsterID(string ID) {
            XmlNodeList MonsterRemovableParts = MonsterDataDocument.SelectNodes($"//Monsters/Monster[@ID='{ID}']/Parts/Part[@IsRemovable='True']");
            if (MonsterRemovableParts == null) return 0;
            int nRemovableParts = MonsterRemovableParts.Count;
            return nRemovableParts;
        }

        static public bool IsPartRemovable(string MonsterID, int PartIndex) {
            XmlNode MonsterPart = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{MonsterID}']/Parts");
            if (MonsterPart == null) return false;
            return bool.Parse(MonsterPart.ChildNodes[PartIndex].Attributes["IsRemovable"].Value);
        }

        static public string GetPartStringIDByPartIndex(string MonsterID, int PartIndex) {
            XmlNode MonsterParts = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{MonsterID}']/Parts");
            if (MonsterParts == null) return "MONSTER_PART_UNKNOWN";
            string PartStringName = MonsterParts.ChildNodes[PartIndex].Attributes["Name"].Value;
            return PartStringName;
        }

        static public string GetPartGroupByPartIndex(string MonsterID, int PartIndex) {
            XmlNode MonsterParts = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{MonsterID}']/Parts");
            if (MonsterParts == null) return "MISC";
            string PartStringName = MonsterParts.ChildNodes[PartIndex].Attributes["Group"].Value;
            return PartStringName;
        }

        static public string GetAilmentIDByIndex(int index) {
            XmlNodeList Ailments = MonsterDataDocument.SelectNodes($"//Monsters/Ailments/Ailment");
            if (Ailments.Count < index) return null;
            return Ailments[index]?.Attributes["Name"].Value;
        }

        static public XmlNode GetAilmentByIndex(int index) {
            XmlNodeList Ailments = MonsterDataDocument.SelectNodes($"//Monsters/Ailments/Ailment");
            if (Ailments.Count < index) return null;
            return Ailments[index];
        }

        static public int GetMonsterCaptureThresholdByID(string MonsterID) {
            XmlNode Monster = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{MonsterID}']");
            if (Monster == null) return 0;
            return int.Parse(Monster.Attributes["Capture"].Value);
        }

    }
}
