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

namespace HunterPie.Plugins
{
    class PluginManager
    {
        public static List<PluginPackage> packages = new List<PluginPackage>();
        internal static Game ctx;

        public void LoadPlugins()
        {
            Stopwatch benchmark = Stopwatch.StartNew();
            if (packages.Count > 0)
            {
                // Quick load
                foreach (PluginPackage package in packages)
                {
                    if (!package.settings.IsEnabled) continue;

                    package.plugin.Initialize(ctx);
                }
            }
            benchmark.Stop();
            Debugger.Module($"Loaded {packages.Count} module(s) in {benchmark.ElapsedMilliseconds}ms");
        }

        public void PreloadPlugins()
        {
            Debugger.Module("Pre loading modules");
            string[] modules = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules"));
            foreach (string module in modules)
            {
                // Skip modules without Module.json
                if (!File.Exists(Path.Combine(module, "module.json"))) continue;

                try
                {
                    string serializedModule = File.ReadAllText(Path.Combine(module, "module.json"));
                    PluginInformation modInformation = JsonConvert.DeserializeObject<PluginInformation>(serializedModule);
                    PluginSettings modSettings = GetPluginSettings(module);

                    if (File.Exists(Path.Combine(module, modInformation.EntryPoint)))
                    {
                        Debugger.Module($"Compiling plugin: {modInformation.Name}");
                        if (CompilePlugin(module, modInformation))
                        {
                            Debugger.Module($"{modInformation.Name} compiled successfully.");
                        }
                        else
                        {
                            continue;
                        }
                    }
                        
                    foreach (string required in modInformation.Dependencies)
                    {
                        AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(Path.Combine(module, required)));
                    }
                    Assembly plugin = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(Path.Combine(module, $"{modInformation.Name}.dll")));
                    IEnumerable<Type> entries = plugin.ExportedTypes.Where(exp => exp.GetMethod("Initialize") != null);
                    if (entries.Count() > 0)
                    {
                        dynamic mod = plugin.CreateInstance(entries.First().ToString());
                        packages.Add(new PluginPackage { plugin = mod, information = modInformation, settings = modSettings, path = module });
                    }
                } catch (Exception err)
                {
                    Debugger.Error(err);
                    continue;
                }
            }
            Debugger.Module($"Pre loaded {packages.Count} module(s)");

        }

        public void UnloadPlugins()
        {
            foreach (PluginPackage package in packages)
            {
                package.plugin.Unload();
            }
            Debugger.Module("Unloaded all modules.");
        }

        public bool CompilePlugin(string pluginPath, PluginInformation information)
        {
            var compiler = CSharpCompilation.Create($"{nameof(HunterPie)}{information.Name}", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release));

            Type[] types = new[]
            {
                typeof(Hunterpie),                  // HunterPie
                typeof(JObject),                 // Newtonsoft.Json.dll
                typeof(Object),                  // mscorlib.dll
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

            if (result.Success)
            {
                return true;
            } else
            {
                Debugger.Error($"Failed to compile plugin: {information.Name}");
                foreach (var exception in result.Diagnostics) Debugger.Error(exception);
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
            } else
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
                plugin.Unload();
                return true;
            } catch(Exception err)
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
            plugin.Initialize(ctx);
            return true;
        }
    }
}
