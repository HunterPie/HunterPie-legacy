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
    class PluginLoader
    {
        List<IPlugin> plugins = new List<IPlugin>();

        public void LoadPlugins(Game ctx)
        {
            Stopwatch benchmark = Stopwatch.StartNew();
            if (plugins.Count > 0)
            {
                // Quick load
                foreach (IPlugin plugin in plugins)
                {
                    plugin.Initialize(ctx);
                }
            }
            else
            {
                string[] modules = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules"));
                foreach (string module in modules)
                {
                    try
                    {
                        string serializedModule = File.ReadAllText(Path.Combine(module, "module.json"));
                        PluginInformation modInformation = JsonConvert.DeserializeObject<PluginInformation>(serializedModule);

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
                        var plugin = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(Path.Combine(module, $"{modInformation.Name}.dll")));
                        IEnumerable<Type> entries = plugin.ExportedTypes.Where(exp => exp.GetMethod("Initialize") != null);
                        if (entries.Count() > 0)
                        {
                            dynamic mod = plugin.CreateInstance(entries.First().ToString());
                            mod.Initialize(ctx);
                            plugins.Add(mod);
                        }
                    } catch (Exception err)
                    {
                        Debugger.Error(err);
                        continue;
                    }
                }
            }
            benchmark.Stop();
            Debugger.Module($"Loaded {plugins.Count} module(s) in {benchmark.ElapsedMilliseconds}ms");
           
        }

        public void UnloadPlugins()
        {
            foreach (IPlugin plugin in plugins)
            {
                plugin.Unload();
            }
            Debugger.Module("Unloaded all modules.");
        }

        public bool CompilePlugin(string pluginPath, PluginInformation information)
        {
            var compiler = CSharpCompilation.Create($"{nameof(HunterPie)}{information.Name}", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release));

            var types = new[]
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
                    references.Add(MetadataReference.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", extDependency)));
                }
                
            }
            compiler = compiler.AddReferences(references);
            var code = File.ReadAllText(Path.Combine(pluginPath, information.EntryPoint));
            var options = CSharpParseOptions.Default.WithLanguageVersion(
                LanguageVersion.CSharp7_3);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code, Encoding.UTF8), options, Path.Combine(pluginPath, information.EntryPoint));

            compiler = compiler.AddSyntaxTrees(syntaxTree);
            var result = compiler.Emit(Path.Combine(pluginPath, information.Name) + ".dll");
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
    }
}
