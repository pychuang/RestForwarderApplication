using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestForwarderApplication.Controllers
{
    [Produces("application/json")]
    [Route("api/data")]
    public class DataplaneController : ControllerBase
    {
        private readonly HttpClient httpMessageInvoker;

        public DataplaneController(HttpClient httpClient)
        {
            this.httpMessageInvoker = httpClient;
        }

        [AcceptVerbs("GET", "POST")]
        [Route("{*containerRequestPath}")]
        public async Task<IActionResult> ForwardRequest([FromRoute] string containerRequestPath, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{this.Request.Method} {this.Request.GetDisplayUrl()}");
            using (HttpRequestMessage requestToHost = new HttpRequestMessage())
            {
                UriBuilder builder = new UriBuilder("http", "localhost", 13765)
                {
                    Path = containerRequestPath
                };

                requestToHost.RequestUri = builder.Uri;
                requestToHost.Method = new HttpMethod(this.Request.Method);
                if (string.Equals(this.Request.Method, "Post", StringComparison.InvariantCultureIgnoreCase))
                {
                    requestToHost.Content = new StreamContent(this.Request.Body);
                }

                /*
                // this works
                using (HttpResponseMessage responseFromHost = await this.httpMessageInvoker.SendAsync(requestToHost, cancellationToken))
                {
                    Stream contentStream = await responseFromHost.Content.ReadAsStreamAsync();
                    MemoryStream newStream = new MemoryStream();
                    await contentStream.CopyToAsync(newStream);
                    newStream.Seek(0, SeekOrigin.Begin);
                    return this.StatusCode((int)responseFromHost.StatusCode, newStream);
                }
                */

                // this works too
                HttpResponseMessage responseFromHost = await this.httpMessageInvoker.SendAsync(requestToHost, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                Stream contentStream = await responseFromHost.Content.ReadAsStreamAsync();
                return this.StatusCode((int)responseFromHost.StatusCode, contentStream);
            }
        }
    }
}
