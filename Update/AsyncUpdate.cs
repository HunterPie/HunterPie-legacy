using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace Update
{
    public struct UpdateFinished
    {
        public bool JustUpdated;
        public bool IsLatestVersion;
    }
    class AsyncUpdate
    {
        static readonly string[] validBranches = { "master", "BETA" };
        static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        private const string UpdateUrl = "https://hunterpie.herokuapp.com/bin/";
        private readonly string updateBranch;
        private string validBranchUrl;

        public int FilesUpdatedCounter { get; private set; }
        public int DifferentFilesCounter { get; private set; }

        public EventHandler<UpdateFinished> OnUpdateFail;
        public EventHandler<UpdateFinished> OnUpdateSuccess;
        public EventHandler<string> OnNewFileUpdate;
        public EventHandler<DownloadProgressChangedEventArgs> OnDownloadProgressChanged;

        private readonly Dictionary<string, string> localFileHashes = new Dictionary<string, string>();
        private Dictionary<string, string> onlineFileHashes;

        public AsyncUpdate(string branch)
        {
            updateBranch = branch;
            BuildValidBranchLink();
        }

        public async Task<bool> Start()
        {   
            if (await GetOnlineFileHashes())
            {
                GetLocalFileHashes();
                string[] files = GetDifferentFiles();
                if (await UpdateFiles(files))
                {
                    OnUpdateSuccess?.Invoke(this, new UpdateFinished { JustUpdated = files.Length > 0, IsLatestVersion = files.Length == 0 });
                }
            }
            return false;
        }

        private void BuildValidBranchLink()
        {
            if (validBranches.Contains(updateBranch))
            {
                validBranchUrl = $"{UpdateUrl}{updateBranch}/";
            } else
            {
                validBranchUrl = $"{UpdateUrl}master/";
            }
        }

        private async Task<bool> GetOnlineFileHashes()
        {
            Message("Looking for online files...");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string hashes = await client.GetStringAsync(new Uri($"{validBranchUrl}hashes.json"));
                    
                    onlineFileHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(hashes);
                }
                return true;
            } catch(Exception err)
            {
                Message("Failed to get online files.");
                // Write to log
                OnUpdateFail?.Invoke(this, new UpdateFinished { IsLatestVersion=true, JustUpdated=false });
                return false;
            }
        }

        private void GetLocalFileHashes()
        {
            Message("Calculating local hashes...");
            Dictionary<string, string>.KeyCollection fileNames = onlineFileHashes.Keys;
            foreach(string fileName in fileNames)
            {
                // Create directories if needed
                CreateDirectories(fileName);

                // Calculate SHA256 hash
                string hash = Hasher.HashFile(Path.Combine(BaseDirectory, fileName));
                localFileHashes[fileName] = hash;
            }
        }

        private void CreateDirectories(string path)
        {
            string dirName = Path.GetDirectoryName(path);
            if (dirName.Length > 0)
            {
                if (!Directory.Exists(Path.Combine(BaseDirectory, dirName)))
                {
                    Directory.CreateDirectory(Path.Combine(BaseDirectory, dirName));
                }
            }
        }

        private string[] GetDifferentFiles()
        {
            List<string> files = new List<string>();
            foreach (string fileName in onlineFileHashes.Keys)
            {
                // Welp
                if (fileName == "Newtonsoft.Json.dll")
                {
                    continue;
                }

                string online = onlineFileHashes[fileName];
                string local = localFileHashes[fileName];

                if (online.ToLowerInvariant() == "installonly" && !string.IsNullOrEmpty(local))
                {
                    continue;
                } else
                {
                    if (online != local)
                    {
                        files.Add(fileName);
                        DifferentFilesCounter++;
                    }
                }
            }
            return files.ToArray();
        }

        private async Task<bool> UpdateFiles(string[] files)
        {
            try
            {
                foreach (string file in files)
                {
                    Message($"Downloading {file.Replace("_", "__")}");
                    Uri link = new Uri($"{validBranchUrl}{file}");
                    using (WebClient client = new WebClient() {
                        Timeout = 10000
                    })
                    {
                        client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        client.Headers.Add(HttpRequestHeader.CacheControl, "no-store");
                        client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                        client.DownloadProgressChanged += OnDownloadProgressChange;

                        byte[] data = await client.DownloadDataTaskAsync(link); 

                        string localFilePath = Path.Combine(BaseDirectory, file);
                        using (FileStream ws = File.OpenWrite(localFilePath))
                        {
                            await ws.WriteAsync(data, 0, data.Length);
                        }
                        FilesUpdatedCounter++;
                        client.DownloadProgressChanged -= OnDownloadProgressChange;
                    }
                }
            } catch
            {
                return false;
            }
            
            return true;
        }

        private void Message(string message) => OnNewFileUpdate?.Invoke(this, message);

        private void OnDownloadProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            OnDownloadProgressChanged?.Invoke(this, e);
        }
    }
}
