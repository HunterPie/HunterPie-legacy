using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xaml;
using HunterPie.Core;
using HunterPie.Logger;
using HunterPie.UI.Infrastructure;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Markdig.Wpf;
using Markdown = Markdig.Wpf.Markdown;
using XamlReader = System.Windows.Markup.XamlReader;
using PluginsControl = HunterPie.GUIControls.Plugins;

namespace HunterPie.Plugins
{
    public class ReadmeViewModel : BaseViewModel
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    Dispatch(() => OnPropertyChanged(nameof(isBusy)));
                }
            }
        }


        public ReadmeViewModel()
        {
            SetEmpty();
        }

        public FlowDocument Document { get; private set; }

        public async void Load(string path)
        {
            cts.Cancel();
            await semaphore.WaitAsync();
            IsBusy = true;

            cts = new CancellationTokenSource();
            var token = cts.Token;

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    await SetEmpty();
                }
                else
                {
                    string content;
                    if (path.StartsWith("file://"))
                    {
                        string fsPath = path.Substring("file://".Length).TrimStart('/');
                        if (!File.Exists(fsPath))
                        {
                            await SetEmpty();
                            return;
                        }
                        content = await LoadFromFilesystem(fsPath);
                    } else
                    {
                        content = await LoadFromUrl(path, token);

                    }
                    await SetContent(content, GetBasePath(path));
                }
            }
            catch (TaskCanceledException)
            {
                // cancelled, will load new image shortly
            }
            catch (Exception ex)
            {
                await SetContent("Error on loading: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
                semaphore.Release();
            }
        }

        private string GetBasePath(string path)
        {
            // returning path without last part:
            //  http://foo.bar/baz/module.json -> http://foo.bar/baz
            //  file://C:\\foo\bar\module.json -> file://C://foo/bar

            path = path.Replace('\\', '/');
            var match = Regex.Match(path, @"[\\\/]", RegexOptions.RightToLeft);
            if (!match.Success) return path;
            return path.Substring(0, match.Index);
        }

        public Task SetEmpty() => SetContent(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_README_EMPTY']"));

        private async Task<string> LoadFromUrl(string url, CancellationToken token)
        {
            using var http = new HttpClient();
            var rsp = await http.GetAsync(url, token).ConfigureAwait(false);
            rsp.EnsureSuccessStatusCode();
            return await rsp.Content.ReadAsStringAsync();
        }
        private static Task<string> LoadFromFilesystem(string path)
        {
            return Task.Run(() =>
            {
                using var fs = File.OpenRead(path);
                using TextReader sr = new StreamReader(fs);
                return sr.ReadToEnd();
            });
        }

        private async Task SetContent(string content, string basePath = null)
        {
            using var reader = await Task.Run(() =>
            {
                var pipeline = BuildPipeline(basePath);
                string xaml = Markdown.ToXaml(content, pipeline);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
                return new XamlXmlReader(stream);
            }).ConfigureAwait(false);

            Dispatch(() =>
            {
                // ReSharper disable once AccessToDisposedClosure - will be executed in sync
                if (XamlReader.Load(reader) is FlowDocument document)
                {
                    Document = document;
                    OnPropertyChanged(nameof(Document));
                    AddImagesZoom();
                }
            });
        }

        public void AddImagesZoom()
        {
            foreach (var image in FindElementOfType<Image>(Document))
            {
                var bind = new InputBinding(PluginsControl.Instance.MagnifyImageCommand, new MouseGesture(MouseAction.LeftClick))
                {
                    CommandParameter = image.Source
                };
                image.InputBindings.Add(bind);
                image.Cursor = Application.Current.TryFindResource("CURSOR_MAGNIFY") as Cursor;
            }
        }

        public IEnumerable<T> FindElementOfType<T>(FlowDocument document) where T : class
        {
            return document.Blocks.SelectMany(FindElementOfType<T>);
        }

        public IEnumerable<T> FindElementOfType<T>(Block block) where T : class
        {
            switch (block)
            {
                case Table table:
                    return table.RowGroups
                        .SelectMany(x => x.Rows)
                        .SelectMany(x => x.Cells)
                        .SelectMany(x => x.Blocks)
                        .SelectMany(FindElementOfType<T>);
                case Paragraph paragraph:
                    return paragraph.Inlines
                        .OfType<InlineUIContainer>()
                        .Where(x => x.Child is T)
                        .Select(x => x.Child as T);
                case BlockUIContainer container:
                    return container.Child is T child
                        ? new[] {child}
                        : new T[0];
                case System.Windows.Documents.List list:
                    return list.ListItems
                        .SelectMany(li => li.Blocks)
                        .SelectMany(FindElementOfType<T>);
                case System.Windows.Documents.Section section:
                    return section.Blocks.SelectMany(FindElementOfType<T>);
                default:
                    Debugger.Warn("Unknown block type: " + block.GetType());
                    return new T[0];
            }
        }

        private static MarkdownPipeline BuildPipeline(string root) => new MarkdownPipelineBuilder()
            .UseSupportedExtensions()
            // this is needed so images with relative paths will be resolved correctly
            .Use(new RelativeLinkParserMdExtension(root))
            .Build();
    }

    public class RelativeLinkParserMdExtension : IMarkdownExtension
    {
        private readonly string root;

        public RelativeLinkParserMdExtension(string root)
        {
            this.root = root;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (string.IsNullOrEmpty(root))
            {
                return;
            }
            pipeline.InlineParsers.Replace<LinkInlineParser>(new RelativeLinkInlineParser(root));
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }
    }

    public class RelativeLinkInlineParser : LinkInlineParser
    {
        private readonly string root;

        public RelativeLinkInlineParser(string root)
        {
            this.root = root;
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var r = base.Match(processor, ref slice);

            // only replace links to images
            if (r && processor.Inline is LinkInline {IsImage: true} link && !string.IsNullOrEmpty(link.Url))
            {
                // only replace relative links
                if (IsAbsoluteUrl(link.Url)) return true;

                var newUrl = root + "/" + link.Url;

                // wpf will throw error if cannot load image from FS, so we will only replace references if file is actually present
                if (newUrl.StartsWith("file://") && !File.Exists(newUrl.Substring(7)))
                {
                    return true;
                }

                link.Url = newUrl;
            }
            return r;
        }

        public static bool IsAbsoluteUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out Uri _);
    }
}
