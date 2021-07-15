using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace LatexClient
{
    public class LatexCompileClient
    {
        private readonly Uri _endpoint;

        public LatexCompileClient(string endpoint)
        {
            _endpoint = new Uri(endpoint);
        }

        /// <summary>
        ///
        /// Raises a FlurlHttpException if the server cannot be reached or the url isn't valid.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<LatexSession> CreateSession(Compiler compiler, string target)
        {
            var url = _endpoint 
                .AppendPathSegment("api")
                .AppendPathSegment("sessions");

            var response = await url.PostJsonAsync(new
            {
                compiler = compiler.ToCompilerString(),
                target = target
            });

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                // Session creation was successful
                var location = response.ResponseMessage.Headers.Location;
                return new LatexSession(_endpoint, location.LocalPath);
            }

            throw new NotImplementedException();
        }

        
    }
}