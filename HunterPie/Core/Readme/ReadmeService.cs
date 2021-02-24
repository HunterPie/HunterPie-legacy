using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HunterPie.Logger;
using HunterPie.Utils;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Markdig.Wpf;

namespace HunterPie.Core.Readme
{
    public class ReadmeService
    {
        private readonly HttpClient http;

        public ReadmeService(HttpClient http)
        {
            this.http = http;
        }

        public ReadmeService()
        {
            this.http = new HttpClient();
        }

        public async Task<ReadmeModel> DownloadReadme(string mdUrl)
        {
            var result = new ReadmeModel
            {
                Readme = await http.GetStringAsync(mdUrl)
            };

            var basePath = UriUtilities.GetBasePath(mdUrl);
            var imageLinks = FindImageLinks(basePath, result.Readme);

            var imagesDict = imageLinks.ToDictionary(kv => kv.Key,
                kv => http.GetByteArrayAsync(kv.Value));
            foreach (var kv in imagesDict)
            {
                try
                {
                    result.Images[kv.Key] = await kv.Value;
                }
                catch (Exception ex)
                {
                    Debugger.Warn($"Error on downloading '{kv.Key}' from '{imageLinks[kv.Key]}': {ex.GetBaseException()}");
                }
            }

            return result;
        }

        public string MdToXaml(string basePath, string md)
        {
            IUrlResolver resolver = GetResolverBasedOnPath(basePath);

            // this is needed so images with relative paths will be resolved correctly
            var parser = new LinkInlineParserEx(resolver);
            var pipeline = BuildPipeline(parser);
            var preprocessedMd = PreprocessMd(md);
            return Markdig.Wpf.Markdown.ToXaml(preprocessedMd, pipeline);
        }

        private static IUrlResolver GetResolverBasedOnPath(string basePath)
        {
            // if base path is unknown, don't do anything with links
            if (string.IsNullOrEmpty(basePath))
            {
                return new PassthroughUrlResolver();
            }

            // if base path is in fs, try to resolve from cache if possible
            if (basePath.StartsWith("file://"))
            {
                return new DownloadedUrlResolver(basePath.Substring(7));
            }

            // if base path is external, resolving only relative links
            return new ExternalUrlResolver(basePath);
        }

        public Dictionary<string, string> FindImageLinks(string urlRoot, string md)
        {
            var resolver = new ExternalUrlResolver(urlRoot);
            var parser = new LinkInlineParserEx(resolver);
            var pipeline = BuildPipeline(parser);
            var preprocessedMd = PreprocessMd(md);
            Markdig.Wpf.Markdown.ToXaml(preprocessedMd, pipeline);
            return resolver.Links;
        }

        private static readonly Regex hideOp = new(@"^\s*<!--<hide>-->?\s*$");
        private static readonly Regex hideCls = new(@"^\s*<!--</hide>-->\s*$");
        private static readonly Regex includeOp = new(@"^\s*<!--<include>\s*$");
        private static readonly Regex includeCls = new(@"^\s*</include>-->\s*$");

        private enum PreprocessState
        {
            Content,
            Hide,
            Include
        }

        private static string PreprocessMd(string md)
        {
            var sb = new StringBuilder();

            var state = PreprocessState.Content;
            var sr = new StringReader(md);
            var line = sr.ReadLine();
            while (line != null)
            {
                switch (state)
                {
                    case PreprocessState.Content:
                        if (hideOp.IsMatch(line)) state = PreprocessState.Hide;
                        else if (includeOp.IsMatch(line)) state = PreprocessState.Include;
                        else sb.AppendLine(line);
                        break;

                    case PreprocessState.Hide:
                        if (hideCls.IsMatch(line)) state = PreprocessState.Content;
                        break;

                    case PreprocessState.Include:
                        if (includeCls.IsMatch(line)) state = PreprocessState.Content;
                        else sb.AppendLine(line);
                        break;
                }

                line = sr.ReadLine();
            }

            return sb.ToString();
        }

        private static MarkdownPipeline BuildPipeline(IMarkdownExtension parser)
        {
            var builder = new MarkdownPipelineBuilder()
                .UseSupportedExtensions();
            // adding extension manually since if type is already present, markdig will not replace it
            builder.Extensions.Add(parser);

            return builder.Build();
        }
    }

    public class LinkInlineParserEx : LinkInlineParser, IMarkdownExtension
    {
        private readonly IUrlResolver resolver;

        public LinkInlineParserEx(IUrlResolver resolver)
        {
            this.resolver = resolver;
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var r = base.Match(processor, ref slice);

            // only replace links to images
            if (r && processor.Inline is LinkInline {IsImage: true} link && !string.IsNullOrEmpty(link.Url))
            {
                // TODO: svg support
                // BitmapImage doesn't support svg, so this is a crude way to avoid errors on rendering
                if (link.Url.EndsWith(".svg"))
                {
                    link.Url = null;
                    return r;
                }

                link.Url = resolver.Resolve(link.Url);
            }

            return r;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) =>
            pipeline.InlineParsers.Replace<LinkInlineParser>(this);

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {}
    }
}
