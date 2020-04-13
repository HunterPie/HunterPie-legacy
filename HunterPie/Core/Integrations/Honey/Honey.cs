using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Debugger = HunterPie.Logger.Debugger;
using GameStructs = HunterPie.Core.LPlayer.GameStructs;
using HunterPie.Properties;

namespace HunterPie.Core {
    public class Honey {

        public static XmlDocument HoneyGearData;
        private static string HoneyLink = "https://honeyhunterworld.com/mhwbi/?";
        // Only calls this if the person pressed the upload build button
        // since I don't want it to be in allocated in memory 100% of the time
        public static void LoadHoneyGearData() {
            HoneyGearData = new XmlDocument();
            HoneyGearData.LoadXml(Resources.HoneyData);
        }

        public static void UnloadHoneyGearData() {
            HoneyGearData = null;
        }

        // Integration with Honey Hunter World peepoHappy
        public static string LinkStructureBuilder(GameStructs.Gear Build) {

            if (HoneyGearData == null) LoadHoneyGearData();

            StringBuilder LinkBuilder = new StringBuilder();

            LinkBuilder.Append(HoneyLink);

            // Basic data
            LinkBuilder.Append(GetWeaponHoneyID(Build.Weapon.Type, Build.Weapon.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Helms", "Helm", Build.Helmet.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Armors", "Armor", Build.Chest.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Arms", "Arm", Build.Hands.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Waists", "Waist", Build.Waist.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Legs", "Leg", Build.Legs.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Charms", "Charm", Build.Charm.ID) + ",");

            // Augments
            for (int AugmentIndex = 0; AugmentIndex < Build.Weapon.NewAugments.Length; AugmentIndex++) {
                LinkBuilder.Append(GetNewAugment(Build.Weapon.NewAugments[AugmentIndex].ID, Build.Weapon.NewAugments[AugmentIndex].Level));
            }

            // Custom Augments
            LinkBuilder.Append("-");
            LinkBuilder.Append(BuildCustomPartsStructure(Build.Weapon.Type, Build.Weapon.CustomAugments));

            // Awakening Skills
            LinkBuilder.Append("-");
            LinkBuilder.Append(BuildAwakeningSkillsStructure(Build.Weapon.Awakenings));

            Debugger.Debug(LinkBuilder);

            UnloadHoneyGearData();

            return null;
        }

        static string GetWeaponHoneyID(int WeaponType, int WeaponID) {
            XmlNodeList nl = HoneyGearData.SelectSingleNode($"//Honey/Weapons").ChildNodes;
            if (WeaponType > nl.Count) return "0";
            XmlNode WeaponNode = nl[WeaponType];
            return WeaponNode.SelectSingleNode($"Weapon[@ID='{WeaponID}']/@HoneyID")?.Value ?? "0";
        }

        static string GetGearHoneyID(string Type, string SubType, int ID) {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/{Type}/{SubType}[@ID='{ID}']/@HoneyID")?.Value;
            return node ?? "0";
        }

        static string GetCharmLevel(int ID) {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/Charms/Charm[@ID='{ID}']/@Level")?.Value;
            return node ?? "0";
        }

        static string GetNewAugment(int Index, byte Level) {
            if (Level == 0) return null;

            string node = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Augments/New[@ID='{Index}']/@HoneyID")?.Value;
            if (node == null) return null;
            return Index == 0 ? $"{node}:{Level}" : $";{node}:{Level}";
        }

        static string BuildCustomPartsStructure(int WeaponType, GameStructs.CustomAugment[] CustomAugments) {
            StringBuilder[] Structure = new StringBuilder[5];
            for (int cAugmentIndex = 0; cAugmentIndex < CustomAugments.Length; cAugmentIndex++) {
                GameStructs.CustomAugment cAugment = CustomAugments[cAugmentIndex];
                // Skip empty slots
                if (cAugment.ID == byte.MaxValue) continue;
                string AugmentType = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Custom").ChildNodes[WeaponType].SelectSingleNode($"Part[@Level='{cAugment.Level + 1}' and @ID='{cAugment.ID}']/@Type")?.Value;
                
                // If we dont find the augment id, then we try the wildcard ones, since there are some
                // missing IDs
                if (AugmentType == null) AugmentType = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Custom").ChildNodes[WeaponType].SelectSingleNode($"Part[@Level='{cAugment.Level + 1}' and @ID='?']/@Type")?.Value;

                // If the augment is still null, then it isn't supported yet. In this case we display an error
                // with the augment ID, so it's easier to map it.
                if (AugmentType == null) {
                    Debugger.Error($"Unsupported custom augment (ID = {cAugment.ID}, Level = {cAugment.Level})");
                    continue;
                }

                // Initializes StringBuilder if it isn't initialized yet
                if (Structure[int.Parse(AugmentType)] == null) Structure[int.Parse(AugmentType)] = new StringBuilder();

                Structure[int.Parse(AugmentType)].Append((cAugment.Level + 1).ToString());
            }

            StringBuilder JoinedResult = new StringBuilder();
            foreach (StringBuilder SubBuilder in Structure) {
                JoinedResult.Append(SubBuilder?.ToString() + ";");
            }
            JoinedResult.Remove(JoinedResult.Length - 1, 1);
            return JoinedResult.ToString();
        }

        static string BuildAwakeningSkillsStructure(GameStructs.AwakenedSkill[] AwakenedSkills) {
            StringBuilder[] Structure = new StringBuilder[5];
            for (int AwakIndex = 0; AwakIndex < 5; AwakIndex++) {
                GameStructs.AwakenedSkill awakened = AwakenedSkills[AwakIndex];

                if (Structure[AwakIndex] == null) Structure[AwakIndex] = new StringBuilder();

                Structure[AwakIndex].Append(HoneyGearData.SelectSingleNode($"//Honey/Weapons/Awakening/Skill[@ID='{awakened.ID}']/@HoneyID")?.Value) ;
            }

            StringBuilder JoinedResult = new StringBuilder();
            foreach (StringBuilder SubBuilder in Structure) {
                JoinedResult.Append(SubBuilder?.ToString() + ";");
            }
            JoinedResult.Remove(JoinedResult.Length - 1, 1);
            return JoinedResult.ToString();
        }

    }
}
