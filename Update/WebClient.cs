using System;
using System.Net;

namespace Update
{
    class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest lWebRequest = base.GetWebRequest(address);
            lWebRequest.Timeout = Timeout;
            ((HttpWebRequest)lWebRequest).CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
            return lWebRequest;
        }
    }
}
