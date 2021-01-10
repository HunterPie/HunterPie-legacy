using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xaml;
using HunterPie.Core;
using Markdig;
using Markdig.Wpf;
using Markdown = Markdig.Wpf.Markdown;
using XamlReader = System.Windows.Markup.XamlReader;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Changelog.xaml
    /// </summary>
    public partial class Changelog : UserControl
    {
        private static Changelog instance;
        public static Changelog Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Changelog();
                }
                return instance;
            }
        }

        public Changelog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private static MarkdownPipeline BuildPipeline() => new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .Build();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string language;

            if (UserSettings.PlayerConfig.HunterPie.Language is null)
                language = "en-us";
            else
                language = UserSettings.PlayerConfig.HunterPie.Language.Split('\\').Last().Replace(".xml", "");

            string changelogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Changelog\\changelog-{language}.md");
            if (!File.Exists(changelogPath))
            {
                changelogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Changelog\\changelog-en-us.md");
            }

            if (!File.Exists(changelogPath)) return;

            string markdown = File.ReadAllText(changelogPath);
            string xaml = Markdown.ToXaml(markdown, BuildPipeline());
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
            {
                using (var reader = new XamlXmlReader(stream, new MyXamlSchemaContext()))
                {
                    if (XamlReader.Load(reader) is FlowDocument document)
                    {
                        Viewer.Document = document;
                    }
                }
            }
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => Process.Start(e.Parameter.ToString());

        class MyXamlSchemaContext : XamlSchemaContext
        {
            public override bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
            {
                if (xamlNamespace.Equals("clr-namespace:Markdig.Wpf", StringComparison.Ordinal))
                {
                    compatibleNamespace = $"clr-namespace:Markdig.Wpf;assembly={Assembly.GetAssembly(typeof(Markdig.Wpf.Styles)).FullName}";
                    return true;
                }
                return base.TryGetCompatibleXamlNamespace(xamlNamespace, out compatibleNamespace);
            }
        }
    }

}
