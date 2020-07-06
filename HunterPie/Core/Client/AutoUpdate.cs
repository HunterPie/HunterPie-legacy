using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using HunterPie.Logger;

namespace HunterPie.Core
{
    class AutoUpdate
    {
        private readonly string BranchURI = "https://hunterpie.herokuapp.com/bin/";
        private string LocalUpdateHash;
        private string OnlineUpdateHash;
        public bool offlineMode;
        public WebClient Instance = new WebClient();

        public AutoUpdate(string branch)
        {
            BranchURI = $"{BranchURI}{branch}/";
            Instance.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
        }

        public bool CheckAutoUpdate()
        {
            CheckLocalHash();
            return CheckOnlineHash();

        }

        private void CheckLocalHash()
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe")))
            {
                LocalUpdateHash = "";
                return;
            }
            using (FileStream _file = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe")))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(_file);

                    StringBuilder builder = new StringBuilder();
                    for (int c = 0; c < bytes.Length; c++)
                    {
                        builder.Append(bytes[c].ToString("x2"));
                    }
                    LocalUpdateHash = builder.ToString();
                }
            }
        }

        private bool CheckOnlineHash()
        {
            try
            {
                Debugger.Update("Checking for new versions of auto-updater");
                return CheckOnlineHash(Instance.DownloadData($"{BranchURI}Update.exe"));
            }
            catch (Exception err)
            {
                Debugger.Error(err);
                Instance.Dispose();
                offlineMode = true;
                return false;
            }
        }

        private bool CheckOnlineHash(byte[] FileData)
        {
            MemoryStream FileBytes = new MemoryStream(FileData);
            using (SHA256 hash = SHA256.Create())
            {
                byte[] computedHash = hash.ComputeHash(FileBytes);
                StringBuilder builder = new StringBuilder();
                for (int c = 0; c < computedHash.Length; c++)
                {
                    builder.Append(computedHash[c].ToString("x2"));
                }
                OnlineUpdateHash = builder.ToString();
            }
            offlineMode = false;
            if (LocalUpdateHash == OnlineUpdateHash || offlineMode)
            {
                Debugger.Update("No newer version found!");
                Instance.Dispose();
                return false;
            }
            DownloadNewUpdater();
            return true;
        }

        private void DownloadNewUpdater()
        {
            Uri UpdateUrl = new Uri($"{BranchURI}Update.exe");
            Instance.DownloadFileAsync(UpdateUrl, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe"));
            Instance.Dispose();
        }
    }
}
