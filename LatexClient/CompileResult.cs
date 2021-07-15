using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;

namespace LatexClient
{
    public class CompileResult
    {
        private readonly FlurlClient _client;
        private readonly SessionInfo _info;

        public CompileResult(FlurlClient client, SessionInfo info)
        {
            _client = client;
            _info = info;
        }

        public bool IsSuccessful => _info.Status == "success";

        public async Task<string> GetLog()
        {
            var result = await _client.Request(_info.Log.Href)
                .GetBytesAsync();

            return Encoding.UTF8.GetString(result);
        }

        public async Task<Stream> GetProduct()
        {
            var result = await _client.Request(_info.Product.Href)
                .GetStreamAsync();
            return result;
        }
    }
}