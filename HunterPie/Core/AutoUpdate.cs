using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System;

namespace HunterPie.Core {
    class AutoUpdate {
        private string BranchURI = "https://bitbucket.org/Haato/hunterpie/raw/";
        private string LocalUpdateHash;
        private string OnlineUpdateHash;
        public bool offlineMode;
        private byte[] FileData;

        public AutoUpdate(string branch) {
            BranchURI = $"{BranchURI}{branch}/";
        }

        public void checkAutoUpdate() {
            CheckLocalHash();
            CheckOnlineHash();
            if (LocalUpdateHash == OnlineUpdateHash || offlineMode) {
                FileData = null; // clear byte array
                return;
            }
            DownloadNewUpdater();
        }

        private void CheckLocalHash() {
            if (!File.Exists("Update.exe")) {
                LocalUpdateHash = "";
                return;
            }
            var _file = File.OpenRead("Update.exe");
            using (SHA256 sha256 = SHA256.Create()) {
                byte[] bytes = sha256.ComputeHash(_file);

                StringBuilder builder = new StringBuilder();
                for (int c = 0; c < bytes.Length; c++) {
                    builder.Append(bytes[c].ToString("x2"));
                }
                _file.Close();
                LocalUpdateHash = builder.ToString();
            }
        }

        private void CheckOnlineHash() {
            try {
                WebClient webClient = new WebClient();
                FileData = webClient.DownloadData($"{BranchURI}Update.exe");
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
            } catch {
                offlineMode = true;
                return;
            }
        }
        
        private void DownloadNewUpdater() {
            using (var UpdateFile = File.OpenWrite("Update.exe")) {
                UpdateFile.Write(FileData, 0, FileData.Length);
            }
        }
    }
}
