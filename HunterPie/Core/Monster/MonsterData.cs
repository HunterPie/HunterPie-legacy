using HunterPie.Properties;
using HunterPie.Logger;
using System.Xml;
using System.Collections.Generic;

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
    }
}
