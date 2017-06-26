using System;
using System.Linq;
using System.Net;
using Nuke.Core.Tooling;

namespace Nuke.Common.IO
{
    public static class HttpTasks
    {
        public static string HttpDownloadString(string uri, Configure<WebClient> configurator = null)
        {
            var webClient = new WebClient();
            webClient = configurator.InvokeSafe(webClient);

            return webClient.DownloadString(new Uri(uri));
        }
        
        public static void HttpDownloadFile(string uri, string path, Configure<WebClient> configurator = null)
        {
            var webClient = new WebClient();
            webClient = configurator.InvokeSafe(webClient);

            webClient.DownloadFile(new Uri(uri), path);
        }
    }
}