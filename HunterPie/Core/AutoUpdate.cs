using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;

namespace HunterPie.Core {
    class AutoUpdate {
        private string BranchURI = "https://bitbucket.org/Haato/hunterpie/raw/";
        private string LocalUpdateHash;
        private string OnlineUpdateHash;
        public bool offlineMode;

        public AutoUpdate(string branch) {
            BranchURI = $"{BranchURI}{branch}/";
        }

        public void checkAutoUpdate() {
            CheckLocalHash();
            CheckOnlineHash();
            if (LocalUpdateHash == OnlineUpdateHash || offlineMode) return;
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
            WebRequest request = WebRequest.Create($"{BranchURI}Update.exe");
            // Check if request was successfully made
            try {
                WebResponse r_response = request.GetResponse();
                using (Stream r_content = r_response.GetResponseStream()) {
                    using (SHA256 sha256 = SHA256.Create()) {
                        byte[] bytes = sha256.ComputeHash(r_content);

                        StringBuilder builder = new StringBuilder();
                        for (int c = 0; c < bytes.Length; c++) {
                            builder.Append(bytes[c].ToString("x2"));
                        }
                        OnlineUpdateHash = builder.ToString();
                    }
                }
                offlineMode = false;
            } catch {
                offlineMode = true;
                return;
            }
        }
        
        private void DownloadNewUpdater() {
            WebClient _WebClient = new WebClient();
            byte[] FileData = _WebClient.DownloadData($"{BranchURI}Update.exe");
            using (var UpdateFile = File.OpenWrite("Update.exe")) {
                UpdateFile.Write(FileData, 0, FileData.Length);
            }
        }
    }
}
