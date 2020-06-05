using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Helpers = HunterPie.Core.Definitions.Helpers;

namespace HunterPie.Core.Integrations.DataExporter
{
    /*
        This class will help HunterPie export the in-game data to a file,
        the file can be used by streamers to share their build, session, in-game data with their viewers
    */
    public struct Data
    {
        public string Name;
        public int HR;
        public int MR;
        public string BuildURL;
        public string Session;
        public string SteamSession;
    }

    public class Exporter
    {
        private readonly string ExportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataExport");

        public Exporter()
        {
            if (!Directory.Exists(ExportPath))
            {
                Directory.CreateDirectory(ExportPath);
            }
        }

        public bool ExportData(Data playerData)
        {
            string filepath = Path.Combine(ExportPath, "PlayerData.json");
            try
            {
                File.WriteAllText(filepath, Helpers.Serialize(playerData));
                return true;
            } catch { return false; }
            
        }
    }
}
