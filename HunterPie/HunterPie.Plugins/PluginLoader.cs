using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using HunterPie.Core;
using HunterPie.Logger;
using Microsoft.CSharp;
using Newtonsoft.Json;

namespace HunterPie.Plugins
{
    class PluginLoader
    {
        List<IPlugin> plugins = new List<IPlugin>();
        Game context;

        public PluginLoader(Game ctx)
        {
            context = ctx;
        }

        public void LoadPlugins()
        {
            Stopwatch benchmark = Stopwatch.StartNew();
            string[] modules = Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules"));
            foreach (string module in modules)
            {
                string serializedModule = File.ReadAllText(Path.Combine(module, "module.json"));
                PluginInformation modInformation = JsonConvert.DeserializeObject<PluginInformation>(serializedModule);

                if (File.Exists(Path.Combine(module, modInformation.EntryPoint))) {
                    Debugger.Module($"Compiling plugin: {modInformation.Name}");
                    if (CompilePlugin(module, modInformation))
                    {
                        Debugger.Module($"{modInformation.Name} compiled successfully.");
                    } else
                    {
                        continue;
                    }
                }
                
                var plugin = Assembly.LoadFile(Path.Combine(module, $"{modInformation.Name}.dll"));
                foreach (Type exported in plugin.ExportedTypes.Where(exp => exp.GetMethod("Initialize") != null))
                {
                    dynamic entry = plugin.CreateInstance(exported.ToString());
                    entry.Initialize(context);
                    plugins.Add(entry);
                }
            }
            benchmark.Stop();
            Debugger.Module($"Loaded {modules.Length} module(s) in {benchmark.ElapsedMilliseconds}ms");
        }

        public void UnloadPlugins()
        {
            foreach (IPlugin plugin in plugins)
            {
                plugin.Unload();
            }
            plugins.Clear();
            Debugger.Module("Unloaded all modules.");
        }

        public bool CompilePlugin(string pluginPath, PluginInformation information)
        {
            var compiler = new CSharpCodeProvider();
            var param = new CompilerParameters();

            var references = new[]
            {
                "System.dll",                                       // System.dll
                typeof(Control).Assembly.Location,                  // PresentationFramework.dll
                typeof(UIElement).Assembly.Location,                // PresentationCore.dll
                typeof(DependencyObject).Assembly.Location,         // WindowsBase.dll
                typeof(Hunterpie).Assembly.Location,                // HunterPie
                typeof(IComponentConnector).Assembly.Location,      // System.Xaml.dll
            };
            param.ReferencedAssemblies.AddRange(references);
            param.OutputAssembly = Path.Combine(pluginPath, $"{information.Name}.dll");

            var code = File.ReadAllText(Path.Combine(pluginPath, information.EntryPoint));
            CompilerResults result = compiler.CompileAssemblyFromSource(param, code);

            if (result.Errors.Count > 0)
            {
                Debugger.Error($"Failed to compile plugin: {information.Name}");
                foreach (var exception in result.Errors) Debugger.Error(exception);
                return false;
            }
            return true;
        }
    }
}
