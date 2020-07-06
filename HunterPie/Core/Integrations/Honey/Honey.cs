using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using Debugger = HunterPie.Logger.Debugger;
using GameStructs = HunterPie.Core.LPlayer.GameStructs;

namespace HunterPie.Core
{
    public class Honey
    {

        public static XmlDocument HoneyGearData;
        private static readonly string HoneyLink = "https://honeyhunterworld.com/mhwbi/?";

        // Only calls this when needed
        // since I don't want it to be allocated in memory 100% of the time
        public static void LoadHoneyGearData()
        {
            HoneyGearData = new XmlDocument();
            HoneyGearData.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.Resources/Data/HoneyData.xml"));
        }

        public static void UnloadHoneyGearData() => HoneyGearData = null;

        /// <summary>
        /// Function to create player build link based on the in-game gear
        /// </summary>
        /// <param name="Build">Player build structure</param>
        /// <param name="ShowErrors">If the function should print errors when they happen (optional)</param>
        /// <returns>Link to the build in Honey Hunters World</returns>
        public static string LinkStructureBuilder(GameStructs.Gear Build, bool ShowErrors = false)
        {

            if (HoneyGearData == null) LoadHoneyGearData();

            StringBuilder LinkBuilder = new StringBuilder();

            LinkBuilder.Append(HoneyLink);

            // Basic data
            LinkBuilder.Append(GetWeaponHoneyID(Build.Weapon.Type, Build.Weapon.ID, ShowErrors) + ",");
            LinkBuilder.Append(GetGearHoneyID("Helms", "Helm", Build.Helmet.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Armors", "Armor", Build.Chest.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Arms", "Arm", Build.Hands.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Waists", "Waist", Build.Waist.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Legs", "Leg", Build.Legs.ID) + ",");
            LinkBuilder.Append(GetGearHoneyID("Charms", "Charm", Build.Charm.ID) + ",");

            // Augments
            int AugmentsTotal = 0;
            for (int AugmentIndex = 0; AugmentIndex < Build.Weapon.NewAugments.Length; AugmentIndex++)
            {
                string AugId = GetNewAugment(Build.Weapon.NewAugments[AugmentIndex].ID);

                if (Build.Weapon.NewAugments[AugmentIndex].Level == 0) continue;
                else { AugmentsTotal++; }

                if (AugmentsTotal > 1)
                {
                    LinkBuilder.Append($";{AugId}:{Build.Weapon.NewAugments[AugmentIndex].Level}");
                }
                else
                {
                    LinkBuilder.Append($"{AugId}:{Build.Weapon.NewAugments[AugmentIndex].Level}");
                }

            }

            // Custom Augments
            LinkBuilder.Append("-");
            LinkBuilder.Append(BuildCustomPartsStructure(Build.Weapon.Type, Build.Weapon.CustomAugments, ShowErrors));

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
            if (Build.Weapon.Type == 12 || Build.Weapon.Type == 13)
            {
                foreach (GameStructs.BowgunMod bowgunMod in Build.Weapon.BowgunMods)
                {
                    LinkBuilder.Append("," + (HoneyGearData.SelectSingleNode($"//Honey/Weapons/BowgunMods/Mod[@ID='{bowgunMod.ID}']/@HoneyID")?.Value ?? "none"));
                }
            }

            if (ShowErrors) Debugger.Debug(LinkBuilder);

            UnloadHoneyGearData();

            return LinkBuilder.ToString();
        }


        static string GetWeaponHoneyID(int WeaponType, int WeaponID, bool ShowErrors)
        {
            XmlNodeList nl = HoneyGearData.SelectSingleNode($"//Honey/Weapons").ChildNodes;
            if (WeaponType > nl.Count) return "0";
            XmlNode WeaponNode = nl[WeaponType];
            string wID = WeaponNode.SelectSingleNode($"Weapon[@ID='{WeaponID}']/@HoneyID")?.Value ?? "0";
            if (wID == "0" && ShowErrors) Debugger.Error($"Unsupported weapon ID: {WeaponID}");
            return wID;
        }

        static string GetGearHoneyID(string Type, string SubType, int ID)
        {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/{Type}/{SubType}[@ID='{ID}']/@HoneyID")?.Value;
            return node ?? "0";
        }

        static int GetCharmLevel(int ID)
        {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/Charms/Charm[@ID='{ID}']/@Level")?.Value;
            int.TryParse(node, out int parsed);
            return parsed;
        }

        static string GetNewAugment(int Index)
        {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Augments/New[@ID='{Index}']/@HoneyID")?.Value;
            if (node == null) return null;
            return node;
        }

        static string BuildCustomPartsStructure(int WeaponType, GameStructs.CustomAugment[] CustomAugments, bool ShowError)
        {
            StringBuilder[] Structure = new StringBuilder[5];
            for (int cAugmentIndex = 0; cAugmentIndex < CustomAugments.Length; cAugmentIndex++)
            {
                GameStructs.CustomAugment cAugment = CustomAugments[cAugmentIndex];
                // Skip empty slots
                if (cAugment.ID == byte.MaxValue) continue;
                string AugmentType = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Custom").ChildNodes[WeaponType].SelectSingleNode($"Part[@Level='{cAugment.Level + 1}' and @ID='{cAugment.ID}']/@Type")?.Value;

                // If we dont find the augment id, then we try the wildcard ones, since there are some
                // missing IDs
                if (AugmentType == null) AugmentType = HoneyGearData.SelectSingleNode($"//Honey/Weapons/Custom").ChildNodes[WeaponType].SelectSingleNode($"Part[@Level='{cAugment.Level + 1}' and @ID='?']/@Type")?.Value;

                // If the augment is still null, then it isn't supported yet. In this case we display an error
                // with the augment ID, so it's easier to map it.
                if (AugmentType == null && ShowError)
                {
                    Debugger.Error($"Unsupported custom augment (ID = {cAugment.ID}, Level = {cAugment.Level})");
                    continue;
                }

                // Initializes StringBuilder if it isn't initialized yet
                int.TryParse(AugmentType, out int parsed);
                if (Structure[parsed] == null) Structure[parsed] = new StringBuilder();

                Structure[parsed].Append((cAugment.Level + 1).ToString());
            }

            StringBuilder JoinedResult = new StringBuilder();
            foreach (StringBuilder SubBuilder in Structure)
            {
                JoinedResult.Append(SubBuilder?.ToString() + ";");
            }
            JoinedResult.Remove(JoinedResult.Length - 1, 1);
            return JoinedResult.ToString();
        }

        static string BuildAwakeningSkillsStructure(GameStructs.AwakenedSkill[] AwakenedSkills)
        {
            StringBuilder[] Structure = new StringBuilder[5];
            for (int AwakIndex = 0; AwakIndex < 5; AwakIndex++)
            {
                GameStructs.AwakenedSkill awakened = AwakenedSkills[AwakIndex];

                if (Structure[AwakIndex] == null) Structure[AwakIndex] = new StringBuilder();

                Structure[AwakIndex].Append(HoneyGearData.SelectSingleNode($"//Honey/Weapons/Awakening/Skill[@ID='{awakened.ID}']/@HoneyID")?.Value);
            }

            StringBuilder JoinedResult = new StringBuilder();
            foreach (StringBuilder SubBuilder in Structure)
            {
                JoinedResult.Append(SubBuilder?.ToString() + ";");
            }
            JoinedResult.Remove(JoinedResult.Length - 1, 1);
            return JoinedResult.ToString();
        }

        static string BuildDecorationStringStructure(GameStructs.Decoration[] Decorations, int Amount = 3, bool isWeapon = false, bool HasDecorationExtraSlot = false)
        {
            StringBuilder stringStructure = new StringBuilder();

            // Shift the jewel to the third slot if it's empty, since Honey uses the
            // Awakening slot always in the third slot
            if (isWeapon && HasDecorationExtraSlot && Decorations[2].ID == int.MaxValue)
            {
                Decorations[2].ID = Decorations[1].ID;
                Decorations[1].ID = int.MaxValue;
            }

            for (int DecoIndex = 0; DecoIndex < Amount; DecoIndex++)
            {
                GameStructs.Decoration deco = Decorations[DecoIndex];
                string decorationHoneyID = deco.ID == int.MaxValue ? "0" : HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@ID='{deco.ID}']/@HoneyID")?.Value;
                stringStructure.Append("," + decorationHoneyID);
            }

            return stringStructure.ToString();
        }

        static string GetMantleHoneyID(int ID)
        {
            string node = HoneyGearData.SelectSingleNode($"//Honey/Gear/Mantles/Mantle[@ID='{ID}']/@HoneyID")?.Value;
            return node ?? "0";
        }

        public static int GetDecorationHoneyIdById(int id)
        {
            string decoHoneyID = id == int.MaxValue ? "0" : HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@ID='{id}']/@HoneyID")?.Value;
            int.TryParse(decoHoneyID, out int parsed);
            return parsed;
        }

        static int GetCharmHoneyIdByGameId(int id)
        {
            string decoHoneyID = id == int.MaxValue ? "0" : HoneyGearData.SelectSingleNode($"//Honey/Gear/Charms/Charm[@ID='{id}']/@HoneyID")?.Value;
            int.TryParse(decoHoneyID, out int parsed);
            return parsed;
        }

        private static int GetDecorationHoneyIdByGameId(int id)
        {
            string decoHoneyId = HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@GameId='{id}']/@HoneyID")?.Value;
            int.TryParse(decoHoneyId, out int parsed);
            return parsed;
        }

        private static int GetDecorationGameIdById(int id)
        {
            string decoGameId = HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@ID='{id}']/@GameId")?.Value;
            int.TryParse(decoGameId, out int parsed);
            return parsed;
        }

        private static int GetDecorationAmountLimit(int id, int amount)
        {
            string decoMax = HoneyGearData.SelectSingleNode($"//Honey/Gear/Jewels/Jewel[@HoneyID='{id}']/@Max")?.Value;
            int.TryParse(decoMax, out int parsed);
            return Math.Min(parsed, amount);
        }

        /// <summary>
        /// Turns a sItem list into a decoration list string that can be used in Honey Hunters World
        /// </summary>
        /// <param name="decorations">sItem array with the decorations information</param>
        /// <param name="gears">sGear array with the gear information</param>
        /// <returns>string structure</returns>
        public static string ExportDecorationsToHoney(sItem[] decorations, sGear[] gears)
        {

            if (HoneyGearData == null) LoadHoneyGearData();
            StringBuilder data = new StringBuilder();

            // Merge decorations in box and decorations in gear
            List<sItem> decoMerge = decorations.ToList<sItem>();

            foreach (sGear gear in gears)
            {
                // Skip gear the player does not have anymore
                if (gear.Category == uint.MaxValue) continue;

                // Skip charms
                if (gear.Category == 2) continue;

                if (gear.DecorationSlot1 != uint.MaxValue)
                {
                    decoMerge.Add(new sItem { Amount = 1, ItemId = GetDecorationGameIdById((int)gear.DecorationSlot1) });
                }
                if (gear.DecorationSlot2 != uint.MaxValue)
                {
                    decoMerge.Add(new sItem { Amount = 1, ItemId = GetDecorationGameIdById((int)gear.DecorationSlot2) });
                }
                if (gear.DecorationSlot3 != uint.MaxValue)
                {
                    decoMerge.Add(new sItem { Amount = 1, ItemId = GetDecorationGameIdById((int)gear.DecorationSlot3) });
                }
            }
            decorations = decoMerge.ToArray<sItem>();

            // Parse decorations into a dictionary to make it easier to organize the string structure
            Dictionary<int, int> sDecorations = new Dictionary<int, int>();
            foreach (sItem deco in decorations)
            {
                int HoneyDecoId = GetDecorationHoneyIdByGameId(deco.ItemId);
                if (sDecorations.ContainsKey(HoneyDecoId))
                {
                    sDecorations[HoneyDecoId] += deco.Amount;
                }
                else
                {
                    sDecorations[HoneyDecoId] = deco.Amount;
                }
            }

            // Now we build the decoration string structure
            const int MaxDecoId = 401;
            for (int i = 1; i <= MaxDecoId; i++)
            {
                data.Append($"{(i != 1 ? "," : "")}{(sDecorations.ContainsKey(i) ? GetDecorationAmountLimit(i, sDecorations[i]) : 0)}");
            }
            Debugger.Debug(data);
            Debugger.Debug($"Total unique decorations found: {sDecorations.Count}");
            UnloadHoneyGearData();
            return data.ToString();
        }

        /// <summary>
        /// Turns a sGear list into a charm list string that can be used in Honey Hunters World
        /// </summary>
        /// <param name="gear">sGear list with the charm information</param>
        /// <returns>string structure</returns>
        public static string ExportCharmsToHoney(sGear[] gear)
        {
            if (HoneyGearData == null) LoadHoneyGearData();

            StringBuilder data = new StringBuilder();

            // Filter based on only on charms
            sGear[] charms = gear.Where(x => x.Type == (uint)GearType.Charm).ToArray();

            // Parse charms into a dictionary to make it easier to organize the string structure
            Dictionary<int, int> sCharms = new Dictionary<int, int>();
            foreach (sGear charm in charms)
            {
                // Check if player doesn't have that gear
                if (charm.Category != 2) continue;

                int HoneyCharmId = GetCharmHoneyIdByGameId(charm.Id);
                int level = GetCharmLevel(charm.Id);
                // unique charms have level 0, but we need them to become 1 in order to Honey recoginize them
                level = level == 0 ? level + 1 : level;

                if (sCharms.ContainsKey(HoneyCharmId))
                {
                    //If the level we find is actually larger, use that instead
                    if (sCharms[HoneyCharmId] < level)
                        sCharms[HoneyCharmId] = level;
                }
                else
                {
                    sCharms[HoneyCharmId] = level;
                }
            }

            // Now we build the charm string structure
            const int MaxCharmId = 108;
            for (int i = 1; i <= MaxCharmId; i++)
            {
                data.Append($"{(i != 1 ? "," : "")}{(sCharms.ContainsKey(i) ? sCharms[i] : 0)}");
            }
            Debugger.Debug(data);
            UnloadHoneyGearData();
            return data.ToString();
        }
    }
}
