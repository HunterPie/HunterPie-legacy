using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xaml;
using Markdig;
using Markdig.Wpf;
using XamlReader = System.Windows.Markup.XamlReader;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Changelog.xaml
    /// </summary>
    public partial class Changelog : UserControl
    {
        private static Changelog _Instance;
        public static Changelog Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Changelog();
                }
                return _Instance;
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

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "changelog.log"))) return;

            var markdown = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "changelog.log"));
            var xaml = Markdig.Wpf.Markdown.ToXaml(markdown, BuildPipeline());
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