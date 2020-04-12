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
        
        // Only calls this if the person pressed the upload build button
        // since I don't want it to be in allocated in memory 100% of the time
        public static void LoadHoneyGearData() {
            HoneyGearData = new XmlDocument();
            HoneyGearData.LoadXml(Resources.HoneyData);
            Debugger.Debug("Loaded Honey Hunter World IDs");
        }

        public static void UnloadHoneyGearData() {
            HoneyGearData = null;
            Debugger.Debug("Unloaded Honey Hunter World IDs");
        }

        // Integration with Honey Hunter World peepoHappy
        public static string LinkStructureBuilder(GameStructs.Gear Build) {

            if (HoneyGearData == null) LoadHoneyGearData();

            StringBuilder LinkBuilder = new StringBuilder();

            UnloadHoneyGearData();

            return null;
        }

    }
}
