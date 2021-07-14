using System.IO;
using System.Reflection;
using Flurl.Http;

namespace LatexClient
{
    public class CompileResult
    {
        private readonly FlurlClient _client;
        private readonly string _productUrl;

        public CompileResult(FlurlClient client, string productUrl, bool isSuccessful)
        {
            _client = client;
            _productUrl = productUrl;
            IsSuccessful = isSuccessful;
        }

        public bool IsSuccessful { get; }

        public string Error { get; set; }

        public Stream GetProductStream()
        {
            var target = _client.

        }
    }
}