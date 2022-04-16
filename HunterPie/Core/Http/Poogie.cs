using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HunterPie.Core.Http
{
    /// <summary>
    /// HunterPie's HTTP client with native support for the Poogie API
    /// </summary>
    public class Poogie : IDisposable
    {
        private HttpClient _client;
        private HttpRequestMessage _request;

        public List<string> Urls { get; set; } = new();
        public string Path { get; set; }
        public HttpMethod Method { get; set; }
        public HttpContent Content { get; set; }
        public TimeSpan Timeout { get; set; }
        public Dictionary<string, string> Headers { get; } = new();

        public async Task<PoogieResponse> RequestAsync()
        {
            foreach (string host in Urls)
            {
                _client = new() { Timeout = Timeout };
                _request = new(Method, $"{host}{Path}");

                foreach (var header in Headers)
                {
                    if (string.IsNullOrEmpty(header.Value))
                        continue;

                    _request.Headers.Add(header.Key, header.Value);
                }

                HttpResponseMessage res;
                try
                {
                    res = await _client.SendAsync(_request);
                }
                catch
                {
                    _client.Dispose();
                    _request.Dispose();

                    continue;
                }

                PoogieResponse response = new(res);
                return response;
            }

            return new PoogieResponse(null);
        }

        public void Dispose()
        {
            _request?.Dispose();
            _client?.Dispose();
        }
    }
}
