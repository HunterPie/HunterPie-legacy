using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xaml;
using HunterPie.Core;
using HunterPie.Core.Readme;
using HunterPie.Logger;
using HunterPie.UI.Infrastructure;
using HunterPie.Utils;
using XamlReader = System.Windows.Markup.XamlReader;
using PluginsControl = HunterPie.GUIControls.Plugins;

namespace HunterPie.Plugins
{
    public class ReadmeViewModel : BaseViewModel
    {
        private CancellationTokenSource cts = new();
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly ReadmeService readmeService;

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


        public ReadmeViewModel(ReadmeService readmeService)
        {
            this.readmeService = readmeService;
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
                    await SetContent(content, UriUtilities.GetBasePath(path));
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
                var xaml = readmeService.MdToXaml(basePath, content);
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

    }
}
