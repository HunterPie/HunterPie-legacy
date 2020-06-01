using System;
using System.Linq;
using System.Text;
using System.Xml;
using Debugger = HunterPie.Logger.Debugger;
using GameStructs = HunterPie.Core.LPlayer.GameStructs;
using System.IO;

namespace HunterPie.Core {
    public class Honey {

        public static XmlDocument HoneyGearData;
        private static string HoneyLink = "https://honeyhunterworld.com/mhwbi/?";
        // Only calls this if the person pressed the upload build button
        // since I don't want it to be allocated in memory 100% of the time
        public static void LoadHoneyGearData() {
            HoneyGearData = new XmlDocument();
            HoneyGearData.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.Resources/Data/HoneyData.xml"));
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
            int AugmentsTotal = 0;
            for (int AugmentIndex = 0; AugmentIndex < Build.Weapon.NewAugments.Length; AugmentIndex++) {
                string AugId = GetNewAugment(Build.Weapon.NewAugments[AugmentIndex].ID);

                if (Build.Weapon.NewAugments[AugmentIndex].Level == 0) continue;
                else { AugmentsTotal++; }

                if (AugmentsTotal > 1) {
                    LinkBuilder.Append($";{AugId}:{Build.Weapon.NewAugments[AugmentIndex].Level}");
                } else {
                    LinkBuilder.Append($"{AugId}:{Build.Weapon.NewAugments[AugmentIndex].Level}");
                }

            }

            // Custom Augments
            LinkBuilder.Append("-");
            LinkBuilder.Append(BuildCustomPartsStructure(Build.Weapon.Type, Build.Weapon.CustomAugments));

            // Awakening Skills
            LinkBuilder.Append("-");
            LinkBuilder.Append(BuildAwakeningSkillsStructure(Build.Weapon.Awakenings));

            LinkBuilder.Append(",0,0");

            // Decorations
            int[] ExtraSlotAwakening = new int[3] { 38, 39, 40 };
            bool HasExtraSlot = Build.Weapon.Awakenings.Where(deco => ExtraSlotAwakening.Contains(deco.ID)).ToArray().Length > 0;
            LinkBuilder.Append(BuildDecorationStringStructure(Build.Weapon.Decorations, isWeapon: true, HasDecorationExtraSlot: HasExtraSlot));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.Helmet.Decorations));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.Chest.Decorations));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.Hands.Decorations));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.Waist.Decorations));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.Legs.Decorations));

            // The rest
            LinkBuilder.Append("," + GetCharmLevel(Build.Charm.ID));
            LinkBuilder.Append(":" + GetMantleHoneyID(Build.SpecializedTools[0].ID));
            LinkBuilder.Append(":" + GetMantleHoneyID(Build.SpecializedTools[1].ID));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.SpecializedTools[0].Decorations, 2).Replace(',', ':'));
            LinkBuilder.Append(BuildDecorationStringStructure(Build.SpecializedTools[1].Decorations, 2).Replace(',', ':'));

            // Bowgun mods
            if (Build.Weapon.Type == 12 || Build.Weapon.Type == 13) {
                foreach (GameStructs.BowgunMod bowgunMod in Build.Weapon.BowgunMods) {
                    LinkBuilder.Append("," + (HoneyGearData.SelectSingleNode($"//Honey/Weapons/BowgunMods/Mod[@ID='{bowgunMod.ID}']/@HoneyID")?.Value ?? "none"));
                }
            }

            Debugger.Debug(LinkBuilder);

            UnloadHoneyGearData();

            return LinkBuilder.ToString();
        }

        static string GetWeaponHoneyID(int WeaponType, int WeaponID) {
            XmlNodeList nl = HoneyGearData.SelectSingleNode($"//Honey/Weapons").ChildNodes;
            if (WeaponType > nl.Count) return "0";
            XmlNode WeaponNode = nl[WeaponType];
            string wID = WeaponNode.SelectSingleNode($"Weapon[@ID='{WeaponID}']/@HoneyID")?.Value ?? "0";
            if (wID == "0") Debugger.Error($"Unsupported weapon ID: {WeaponID}");
            return wID;
        }

        static string GetGearHoneyID(string Type, string SubType, int ID) {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/{Type}/{SubType}[@ID='{ID}']/@HoneyID")?.Value;
            return node ?? "0";
        }

        static string GetCharmLevel(int ID) {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/Charms/Charm[@ID='{ID}']/@Level")?.Value;
            return node ?? "0";
        }

        static string GetNewAugment(int Index) {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Augments/New[@ID='{Index}']/@HoneyID")?.Value;
            if (node == null) return null;
            return node;
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

                Structure[AwakIndex].Append(HoneyGearData.SelectSingleNode($"//Honey/Weapons/Awakening/Skill[@ID='{awakened.ID}']/@HoneyID")?.Value);
            }

            StringBuilder JoinedResult = new StringBuilder();
            foreach (StringBuilder SubBuilder in Structure) {
                JoinedResult.Append(SubBuilder?.ToString() + ";");
            }
            JoinedResult.Remove(JoinedResult.Length - 1, 1);
            return JoinedResult.ToString();
        }

        static string BuildDecorationStringStructure(GameStructs.Decoration[] Decorations, int Amount = 3, bool isWeapon = false, bool HasDecorationExtraSlot = false) {
            StringBuilder stringStructure = new StringBuilder();

            // Shift the jewel to the third slot if it's empty, since Honey uses the
            // Awakening slot always in the third slot
            if (isWeapon && HasDecorationExtraSlot && Decorations[2].ID == int.MaxValue) {
                Decorations[2].ID = Decorations[1].ID;
                Decorations[1].ID = int.MaxValue;
            }

            for (int DecoIndex = 0; DecoIndex < Amount; DecoIndex++) {
                GameStructs.Decoration deco = Decorations[DecoIndex];
                string decorationHoneyID = deco.ID == int.MaxValue ? "0" : HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@ID='{deco.ID}']/@HoneyID")?.Value;
                stringStructure.Append("," + decorationHoneyID);
            }
            
            return stringStructure.ToString();
        }

        static string GetMantleHoneyID(int ID) {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/Mantles/Mantle[@ID='{ID}']/@HoneyID")?.Value;
            return node ?? "0";
        }

        static string GetDecorationHoneyID(GameStructs.Decoration decoration)
        {
            string decoHoneyID = decoration.ID == int.MaxValue ? "0" : HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@ID='{decoration.ID}']/@HoneyID")?.Value;
            return decoHoneyID;
        }
    }
}
