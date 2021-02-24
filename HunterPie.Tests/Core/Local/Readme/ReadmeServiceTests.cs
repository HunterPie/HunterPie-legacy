using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HunterPie.Core.Readme;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace HunterPie.Tests.Core.Readme
{
    [TestFixture]
    public class ReadmeServiceTests
    {
        [Test]
        public void ShouldUseConditionalTags()
        {
            // arrange
            var sut = new ReadmeService();
            string input = LoadReadme();

            // act
            var result = sut.MdToXaml("http://example.com", input);
            Console.WriteLine(result);

            // assert
            Assert.IsTrue(result.Contains("this text will be included in GitHub and HunterPie"));
            Assert.IsTrue(result.Contains("this text will be included only in HunterPie"));
            Assert.IsFalse(result.Contains("this text will be included only in GitHub"));
        }

        [Test]
        public void ShouldFindAllImagesLinks()
        {
            // arrange
            var sut = new ReadmeService();
            string input = LoadReadme();

            // act
            var result = sut.FindImageLinks("http://example.com/t", input);

            // assert
            var expected = new Dictionary<string, string>
            {
                {
                    "https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png",
                    "https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png"
                },
                { "./relative1.png", "http://example.com/t/relative1.png" },
                { "./relative2.png", "http://example.com/t/relative2.png" },
                { "relative3.png", "http://example.com/t/relative3.png" },
            };
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReplaceImageLinksWithAbsoluteLink()
        {
            // arrange
            var sut = new ReadmeService();
            var input = LoadReadme();

            // act
            var result = sut.MdToXaml("http://example.com/t", input);
            Console.WriteLine(result);

            // assert
            Assert.IsTrue(result.Contains("https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png"));
            Assert.IsTrue(result.Contains("http://example.com/t/relative1.png"));
            Assert.IsTrue(result.Contains("http://example.com/t/relative2.png"));
            Assert.IsTrue(result.Contains("http://example.com/t/relative3.png"));
        }

        [Test]
        public async Task ShouldDownloadReadme()
        {
            // arrange
            var readme = "![](./img1.png)\n![](./img2.png)";
            var img1Bytes = new byte[] {255, 1};
            var img2Bytes = new byte[] {255, 2};
            var httpClient = CreateMockedHttpClient(new Dictionary<string, HttpContent>
            {
                { "https://example.com/t/readme.md", new StringContent(readme) },
                { "https://example.com/t/img1.png", new ByteArrayContent(img1Bytes) },
                { "https://example.com/t/img2.png", new ByteArrayContent(img2Bytes) },
            });
            var sut = new ReadmeService(httpClient);

            // act
            var result = await sut.DownloadReadme("https://example.com/t/readme.md");

            //assert
            Assert.AreEqual(readme, result.Readme);
            Assert.That(result.Images, Is.EqualTo(new Dictionary<string, byte[]>
            {
                { "./img1.png", img1Bytes },
                { "./img2.png", img2Bytes },
            }));
        }

        private HttpClient CreateMockedHttpClient(Dictionary<string, HttpContent> responses)
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();

            foreach (var kv in responses)
            {
                mockMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.ToString() == kv.Key),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = kv.Value
                    });
            }

            return new HttpClient(mockMessageHandler.Object);
        }

        private static string LoadReadme()
        {
            using var sr = new StreamReader(
                typeof(ReadmeServiceTests).Assembly.GetManifestResourceStream("HunterPie.Tests.Data.readme.md")
            );
            var input = sr.ReadToEnd();
            return input;
        }
    }
}
