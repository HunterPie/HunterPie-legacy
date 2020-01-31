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
            GetMonsterWeaknessById("em114_00");
        }

        static public Dictionary<string, int> GetMonsterWeaknessById(string ID) {
            Dictionary<string, int> MonsterWeaknesses = new Dictionary<string, int>();
            XmlNode Weaknesses = MonsterDataDocument.SelectSingleNode($"//Monsters/Monster[@ID='{ID}']/Weaknesses");
            if (Weaknesses == null) return null;
            foreach (XmlNode Weakness in Weaknesses) {
                MonsterWeaknesses.Add(Weakness.Attributes["ID"].Value, System.Convert.ToInt32(Weakness.Attributes["Stars"].Value));
            }
            return MonsterWeaknesses;
        }
    }
}
