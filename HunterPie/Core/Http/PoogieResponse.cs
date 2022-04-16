using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HunterPie.Core.Http
{
    public class PoogieResponse : IDisposable
    {
        private HttpResponseMessage _response;
        public HttpStatusCode Status { get; }
        public HttpContent Content { get; }
        public bool Success { get; }
        public string Url { get; }


        public PoogieResponse(HttpResponseMessage message)
        {
            if (message is null)
            {
                Success = false;
                return;
            }

            _response = message;
            Status = message.StatusCode;
            Content = message.Content;
            Success = true;
            Url = message.RequestMessage.RequestUri.AbsoluteUri;
        }

        public async Task<T> AsJson<T>()
        {
            string content = await Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<byte[]> AsRaw()
        {
            return await Content.ReadAsByteArrayAsync();
        }

        public void Dispose()
        {
            _response?.Dispose();
        }
    }
}
