using System;
using System.IO;
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
        public int Playtime;
        public string WeaponName;
    }

    public class Exporter
    {
        public readonly string ExportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataExport");

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
            }
            catch { return false; }

        }

        /// <summary>
        /// Function to export custom data to the ExportedData folder
        /// </summary>
        /// <param name="path">Relative path to the file</param>
        /// <param name="data">File content to be exported</param>
        /// <returns>True if the export was successful, false if not</returns>
        public bool ExportCustomData(string path, string data)
        {
            string filepath = Path.Combine(ExportPath, path);

            try
            {
                File.WriteAllText(filepath, data);
                return true;
            }
            catch { return false; };
        }
    }
}
