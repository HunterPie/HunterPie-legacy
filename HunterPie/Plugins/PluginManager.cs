using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using HunterPie.Core;
using Debugger = HunterPie.Logger.Debugger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Numerics;
using System.Windows.Forms;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;
using HunterPie.GUI;

namespace HunterPie.Plugins
{
    class PluginManager
    {
        public static List<PluginPackage> packages = new List<PluginPackage>();
        public bool IsReady { get; private set; }
        internal bool QueueLoad { get; set; }
        internal static Game ctx;

        private static readonly HashSet<string> failedPlugins = new HashSet<string>();
        private static readonly TaskCompletionSource<object> tsc = new TaskCompletionSource<object>();
        internal static readonly Task PreloadTask = tsc.Task;

        public void LoadPlugins()
        {
            Stopwatch benchmark = Stopwatch.StartNew();
            if (packages.Count > 0)
            {
                // Quick load
                foreach (PluginPackage package in packages)
                {
                    if (!package.settings.IsEnabled) continue;

                    try
                    {
                        package.plugin.Initialize(ctx);
                    }
                    catch (Exception err)
                    {
                        Debugger.Error(err);
                    }

                }
            }
            benchmark.Stop();
            Debugger.Module($"Loaded {packages.Count} module(s) in {benchmark.ElapsedMilliseconds}ms");
        }

        public async Task<bool> PreloadPlugins()
        {
            Stopwatch benchmark = Stopwatch.StartNew();
            Debugger.Module("Pre loading modules");
            foreach (string module in IterateModules())
            {
                try
                {
                    string serializedModule = File.ReadAllText(Path.Combine(module, "module.json"));
                    PluginInformation modInformation = JsonConvert.DeserializeObject<PluginInformation>(serializedModule);

                    try
                    {
                        await PreloadPlugin(module, modInformation);
                        failedPlugins.Remove(modInformation.Name);
                    }
                    catch
                    {
                        failedPlugins.Add(modInformation.Name);
                        throw;
                    }
                }
                catch (Exception err)
                {
                    Debugger.Error(err);
                }
            }

            IsReady = true;
            benchmark.Stop();
            Debugger.Module($"Pre loaded {packages.Count} module(s) in {benchmark.ElapsedMilliseconds}ms");
            if (QueueLoad)
            {
                LoadPlugins();
            }

            if (!tsc.Task.IsCompleted)
            {
                tsc.SetResult(null);
            }
            return true;
        }

        /// <summary>
        /// Returns paths to all valid module directories.
        /// </summary>
        private static IEnumerable<string> IterateModules() {

            string[] modules = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules"));
            foreach (string module in modules)
            {
                // Skip modules without Module.json
                if (!File.Exists(Path.Combine(module, "module.json"))) continue;
                yield return module;
            }
        }

        private async Task PreloadPlugin(string module, PluginInformation modInformation)
        {
            if (File.Exists(Path.Combine(module, ".remove")))
            {
                Directory.Delete(module, true);
                Debugger.Module($"Plugin {modInformation.Name} removed.");
                return;
            }

            if (modInformation.Update.MinimumVersion is null)
            {
                Debugger.Error($"{modInformation.Name.ToUpper()} MIGHT BE OUTDATED! CONSIDER UPDATING IT.");
            }

            if (PluginUpdate.PluginSupportsUpdate(modInformation))
            {
                switch (await PluginUpdate.UpdateAllFiles(modInformation, module))
                {
                    case UpdateResult.Updated:
                        var serializedModule = File.ReadAllText(Path.Combine(module, "module.json"));
                        modInformation = JsonConvert.DeserializeObject<PluginInformation>(serializedModule);

                        Debugger.Module($"Updated plugin: {modInformation.Name} (ver {modInformation.Version})");
                        break;

                    case UpdateResult.Skipped:
                        break;

                    case UpdateResult.Failed:
                        Debugger.Error($"Failed to update plugin: {modInformation.Name}");
                        break;

                    case UpdateResult.UpToDate:
                        Debugger.Module($"Plugin {modInformation.Name} is up-to-date (ver {modInformation.Version})");
                        break;
                }
            }

            PluginSettings modSettings = GetPluginSettings(module);

            if (!string.IsNullOrEmpty(modInformation.EntryPoint) &&
                File.Exists(Path.Combine(module, modInformation.EntryPoint)))
            {
                Debugger.Module($"Compiling plugin: {modInformation.Name}");
                if (CompilePlugin(module, modInformation))
                {
                    Debugger.Module($"{modInformation.Name} compiled successfully.");
                }
                else
                {
                    return;
                }
            }

            foreach (string required in modInformation.Dependencies)
            {
                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(Path.Combine(module, required)));
            }

            Assembly plugin =
                AppDomain.CurrentDomain.Load(
                    AssemblyName.GetAssemblyName(Path.Combine(module, $"{modInformation.Name}.dll")));
            IEnumerable<Type> entries = plugin.ExportedTypes.Where(exp => exp.GetMethod("Initialize") != null);

            if (entries.Any())
            {
                dynamic mod = plugin.CreateInstance(entries.First().ToString());
                packages.Add(new PluginPackage
                {
                    plugin = mod, information = modInformation, settings = modSettings, path = module
                });
            }
        }

        public void UnloadPlugins()
        {
            foreach (PluginPackage package in packages)
            {
                UnloadPlugin(package.plugin);
            }
            Debugger.Module("Unloaded all modules.");
        }

        public bool CompilePlugin(string pluginPath, PluginInformation information)
        {

            var compiler = CSharpCompilation.Create($"{nameof(HunterPie)}{information.Name}", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release));

            Type[] types = new[]
            {
                typeof(Player),                  // HunterPie.Core.dll
                typeof(Overlay),                 // HunterPie.UI.dll
                typeof(JObject),                 // Newtonsoft.Json.dll
                typeof(object),                  // mscorlib.dll
                typeof(UIElement),               // PresentationCore.dll
                typeof(Window),                  // PresentationFramework.dll
                typeof(Uri),                     // System.dll
                typeof(Enumerable),              // System.Core.dll
                typeof(DataSet),                 // System.Data.dll
                typeof(DataTableExtensions),     // System.Data.DataSetExtensions.dll
                typeof(Bitmap),                  // System.Drawing.dll
                typeof(HttpClient),              // System.Net.Http.dll
                typeof(BigInteger),              // System.Numerics.dll
                typeof(Form),                    // System.Windows.Forms.dll
                typeof(XamlType),                // System.Xaml.dll
                typeof(XmlNode),                 // System.Xml.dll
                typeof(XNode),                   // System.Xml.Linq.dll
                typeof(Rect),                    // WindowsBase.dll
            };

            // Load all basic dependencies
            List<MetadataReference> references = types.Select(type => MetadataReference.CreateFromFile(type.Assembly.Location)).ToList<MetadataReference>();

            if (information.Dependencies != null)
            {
                foreach (string extDependency in information.Dependencies)
                {
                    references.Add(MetadataReference.CreateFromFile(Path.Combine(pluginPath, extDependency)));
                }

            }
            compiler = compiler.AddReferences(references);
            string code = File.ReadAllText(Path.Combine(pluginPath, information.EntryPoint));

            CSharpParseOptions options = CSharpParseOptions.Default.WithLanguageVersion(
                LanguageVersion.CSharp7_3);

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code, Encoding.UTF8), options, Path.Combine(pluginPath, information.EntryPoint));

            compiler = compiler.AddSyntaxTrees(syntaxTree);

            Microsoft.CodeAnalysis.Emit.EmitResult result = compiler.Emit(Path.Combine(pluginPath, information.Name) + ".dll");

            code = null;
            syntaxTree = null;
            compiler = null;
            references.Clear();
            references = null;

            if (result.Success)
            {
                result = null;
                return true;
            }
            else
            {
                Debugger.Error($"Failed to compile plugin: {information.Name}");
                foreach (Diagnostic exception in result.Diagnostics)
                {
                    Debugger.Error(exception);
                }
                result = null;
                return false;
            }

        }

        internal static PluginSettings GetPluginSettings(string path)
        {
            PluginSettings settings;
            if (!File.Exists(Path.Combine(path, "plugin.settings.json")))
            {
                settings = new PluginSettings();

                File.WriteAllText(Path.Combine(path, "plugin.settings.json"),
                    JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented));
            }
            else
            {
                settings = JsonConvert.DeserializeObject<PluginSettings>(File.ReadAllText(Path.Combine(path, "plugin.settings.json")));
            }
            return settings;
        }

        internal static void UpdatePluginSettings(string path, PluginSettings newSettings)
        {
            File.WriteAllText(Path.Combine(path, "plugin.settings.json"), JsonConvert.SerializeObject(newSettings, Newtonsoft.Json.Formatting.Indented));
        }

        /// <summary>
        /// Unloads a specific plugin
        /// </summary>
        /// <param name="plugin">The plugin to be unloaded</param>
        /// <returns>True if the plugin was unloaded successfully, false otherwise</returns>
        public static bool UnloadPlugin(IPlugin plugin)
        {
            if (ctx == null) return true;
            try
            {
                // This means this plugin is not loaded, so we skip it
                if (plugin.Context == null)
                {
                    return true;
                }

                plugin.Unload();
                return true;
            }
            catch (Exception err)
            {
                Debugger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Loads a specific plugin
        /// </summary>
        /// <param name="plugin">Plugin to be loaded</param>
        /// <returns>True if it was loaded successfully, false if not</returns>
        public static bool LoadPlugin(IPlugin plugin)
        {
            if (ctx == null) return false;
            var name = packages.FirstOrDefault(p => p.plugin == plugin).information?.Name;
            try
            {
                plugin.Initialize(ctx);
                if (!string.IsNullOrEmpty(name)) failedPlugins.Remove(name);
            }
            catch (Exception ex)
            {
                Debugger.Error($"Error on initializing plugin {plugin.Name}: {ex}");
                if (!string.IsNullOrEmpty(name)) failedPlugins.Add(name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if <plugin_name>/module.json is present
        /// </summary>
        internal static bool IsInstalled(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName)) return false;
            return File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", pluginName, "module.json"));
        }

        /// <summary>
        /// Returns true if <plugin_name>/.remove is present so plugin should be removed on next launch
        /// </summary>
        public static bool IsMarkedForRemoval(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName)) return false;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", pluginName, ".remove");
            return File.Exists(path);
        }

        /// <summary>
        /// Deletes plugin directory. Can be safely done only if plugin isn't loaded into AppDomain since it's files will be in use
        /// </summary>
        public static void DeleteNonPreloadedPlugin(string pluginName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", pluginName);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Marking file for removal, so it will be removed on next launch
        /// </summary>
        public static void MarkForRemoval(string pluginName, bool mark = true)
        {
            var pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", pluginName);
            if (!Directory.Exists(pluginDir))
            {
                return;
            }
            string path = Path.Combine(pluginDir, ".remove");
            if (mark)
            {
                File.WriteAllText(path, "");
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Returns information about all installed modules
        /// </summary>
        public static IEnumerable<PluginEntry> GetAllPlugins()
        {
            foreach (var module in IterateModules())
            {
                var existing = packages.FirstOrDefault(p => p.path == module);
                if (!existing.Equals(default(PluginPackage)))
                {
                    yield return new PluginEntry {Package = existing, PluginInformation = existing.information, RootPath = module, IsFailed = IsFailed(existing.information.Name)};
                }
                else
                {
                    string serializedModule = File.ReadAllText(Path.Combine(module, "module.json"));
                    var modInformation = JsonConvert.DeserializeObject<PluginInformation>(serializedModule);
                    yield return new PluginEntry {Package = null, PluginInformation = modInformation, RootPath = module, IsFailed = IsFailed(modInformation.Name)};
                }
            }
        }

        public static bool IsFailed(string pluginName) => failedPlugins.Contains(pluginName);
    }
}
