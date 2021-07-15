using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;

namespace LatexClient
{
    public interface ICompileResult
    {
        /// <summary>
        /// Gets whether or not the compilation completed successfully. If the compilation was not successful
        /// there will be no product.
        /// </summary>
        bool IsSuccessful { get; }

        /// <summary>
        /// Asynchronously retrieve the compilation log, which can be used to debug errors in the case that the
        /// compilation failed.
        /// </summary>
        /// <returns>A string containing the entire compiler log</returns>
        Task<string> GetLog();

        /// <summary>
        /// Asynchronously retrieve the raw binary product of compilation. The file data streamed will be of the format
        /// specified in the session parameters.  If the compilation result was not successful, this will throw a
        /// FIleNotFoundException.  
        /// </summary>
        /// <returns>A stream with the file contents</returns>
        Task<Stream> GetProduct();
    }

    /// <summary>
    /// A handle to the results of a compilation. Can be used to access the result state, the log data,
    /// and the resulting product if there is one.
    /// </summary>
    internal class CompileResult : ICompileResult
    {
        private readonly FlurlClient _client;
        private readonly SessionInfo _info;

        public CompileResult(FlurlClient client, SessionInfo info)
        {
            _client = client;
            _info = info;
        }

        /// <summary>
        /// Gets whether or not the compilation completed successfully. If the compilation was not successful
        /// there will be no product.
        /// </summary>
        public bool IsSuccessful => _info.Status == "success";

        /// <summary>
        /// Asynchronously retrieve the compilation log, which can be used to debug errors in the case that the
        /// compilation failed.
        /// </summary>
        /// <returns>A string containing the entire compiler log</returns>
        public async Task<string> GetLog()
        {
            var result = await _client.Request(_info.Log.Href)
                .GetBytesAsync();

            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Asynchronously retrieve the raw binary product of compilation. The file data streamed will be of the format
        /// specified in the session parameters.  If the compilation result was not successful, this will throw a
        /// FIleNotFoundException.  
        /// </summary>
        /// <returns>A stream with the file contents</returns>
        public async Task<Stream> GetProduct()
        {
            if (!IsSuccessful) 
                throw new FileNotFoundException(
                    "The compilation was not successful, so there is not product file to retrieve. Check the IsSuccessful flag before attempting to get the product");

            var result = await _client.Request(_info.Product.Href)
                .GetStreamAsync();
            return result;
        }
    }
}