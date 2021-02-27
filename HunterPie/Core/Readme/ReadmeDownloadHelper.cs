using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace HunterPie.Core.Readme
{
    public static class ReadmeDownloadHelper
    {
        public static async Task DownloadReadme(ReadmeService readmeService, string modPath, string readmeUrl, BitmapImage icon)
        {
            var readmeModel = await readmeService.DownloadReadme(readmeUrl);
            File.WriteAllText(Path.Combine(modPath, "README.md"), readmeModel.Readme);

            // if there are images in readme, saving them to cache
            if (readmeModel.Images.Count > 0)
            {
                var cachePath = Path.Combine(modPath, ".cache");
                var indexPath = Path.Combine(cachePath, "index.json");

                // create cache dir if missing
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);

                // read existing index
                ReadmeIndex index = new();
                if (File.Exists(indexPath))
                    index = JsonConvert.DeserializeObject<ReadmeIndex>(File.ReadAllText(indexPath));
                index.Source = readmeUrl;

                var previousImages = index.Images.ToDictionary(kv => kv.Key, kv => kv.Value);

                // saving downloaded images to cache
                foreach (var kv in readmeModel.Images)
                {
                    var cacheKey = $"{Guid.NewGuid()}{Path.GetExtension(kv.Key)}";
                    var fileName = Path.Combine(cachePath, cacheKey);
                    File.WriteAllBytes(fileName, kv.Value);
                    index.Images[kv.Key] = cacheKey;
                }

                // save icon
                if (icon != null)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(icon));

                    var iconPath = Path.Combine(cachePath, "icon.png");
                    using var fs = File.OpenWrite(iconPath);
                    encoder.Save(fs);
                }

                // writing result index
                File.WriteAllText(indexPath, JsonConvert.SerializeObject(index, Formatting.Indented));

                // removing old images
                foreach (var kv in previousImages)
                {
                    if (index.Images.ContainsKey(kv.Key)) continue;

                    var path = Path.Combine(cachePath, kv.Value);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }
    }

}
