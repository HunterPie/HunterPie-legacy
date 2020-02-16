using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System;
using HunterPie.Logger;

namespace HunterPie.Core {
    class AutoUpdate {
        private string BranchURI = "https://bitbucket.org/Haato/hunterpie/raw/";
        private string LocalUpdateHash;
        private string OnlineUpdateHash;
        public bool offlineMode;
        private byte[] FileData;
        public WebClient Instance = new WebClient();

        public AutoUpdate(string branch) {
            BranchURI = $"{BranchURI}{branch}/";
        }

        public void checkAutoUpdate() {
            CheckLocalHash();
            CheckOnlineHash();
        }

        private void CheckLocalHash() {
            if (!File.Exists("Update.exe")) {
                LocalUpdateHash = "";
                return;
            }
            using (var _file = File.OpenRead("Update.exe")) {
                using (SHA256 sha256 = SHA256.Create()) {
                    byte[] bytes = sha256.ComputeHash(_file);

                    StringBuilder builder = new StringBuilder();
                    for (int c = 0; c < bytes.Length; c++) {
                        builder.Append(bytes[c].ToString("x2"));
                    }
                    LocalUpdateHash = builder.ToString();
                }
            }
        }

        private void CheckOnlineHash() {
            try {
                Uri UpdateUrl = new Uri($"{BranchURI}Update.exe");
                Debugger.Update("Checking for new versions of auto-updater");
                Instance.DownloadDataCompleted += OnDownloadDataCompleted;
                Instance.DownloadDataAsync(UpdateUrl);
            } catch {
                Instance.Dispose();
                offlineMode = true;
                return;
            }
        }

        private void OnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e) {
            FileData = e.Result;
            MemoryStream FileBytes = new MemoryStream(FileData);
            using (SHA256 hash = SHA256.Create()) {
                byte[] computedHash = hash.ComputeHash(FileBytes);
                StringBuilder builder = new StringBuilder();
                for (int c = 0; c < computedHash.Length; c++) {
                    builder.Append(computedHash[c].ToString("x2"));
                }
                OnlineUpdateHash = builder.ToString();
            }
            offlineMode = false;
            if (LocalUpdateHash == OnlineUpdateHash || offlineMode) {
                Debugger.Update("No newer version found!");
                FileData = null; // clear byte array
                Instance.Dispose();
                return;
            }
            DownloadNewUpdater();
        }

        private void DownloadNewUpdater() {
            File.WriteAllBytes("Update.exe", FileData);
            FileData = null;
        }
    }
}
