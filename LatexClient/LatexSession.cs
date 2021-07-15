using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LatexClient
{
    public interface ILatexSession
    {
        /// <summary>
        /// Get the full URI of the session resource on the server
        /// </summary>
        Uri SessionUri { get; }

        /// <summary>
        /// Asynchronously get the session information from the server
        /// </summary>
        /// <returns></returns>
        Task<SessionInfo> GetInfo();

        /// <summary>
        /// Upload a file to the session from a stream. The filename provided is the filename to save the data to
        /// within the session, and can include subdirectory path elements if necessary. The server will not preserve
        /// paths which attempt to leave the session folder.
        /// </summary>
        /// <param name="fileName">The filename and relative path to save the file as within the session</param>
        /// <param name="stream">A stream containing the file data</param>
        /// <returns></returns>
        Task UploadFile(string fileName, Stream stream);

        /// <summary>
        /// Upload a file to the session from a local file. The filename provided is the filename to save the data to
        /// within the session, and can include sub-directory path elements if necessary. The server will not preserve
        /// paths which attempt to leave the session folder.  The file path provided is the path to the file locally,
        /// which will be opened in read-only mode and its contents sent to the server.
        /// </summary>
        /// <param name="fileName">The filename and relative path to save the file as within the session</param>
        /// <param name="filePath">A path to the file locally which will be opened and saved to the server</param>
        /// <returns></returns>
        Task UploadFile(string fileName, string filePath);

        /// <summary>
        /// Upload a template to the session. The target name provided is the file to which the rendered template will
        /// be saved within the session. The template text is the contents of the template file, and the data payload is
        /// an object which will be serialized to JSON and contains the structure for the Jinja2 rendering engine to
        /// insert into the template.
        /// </summary>
        /// <param name="targetName">The name to save the file to after rendering</param>
        /// <param name="templateText">The text contents of the template file</param>
        /// <param name="dataPayload">An object containing the data to be rendered into the template</param>
        /// <returns></returns>
        Task UploadTemplate(string targetName, string templateText, object dataPayload);

        /// <summary>
        /// Set the session to convert the final product to an image format after compilation. Be aware that using
        /// high dpi settings and/or converting large (format) files can result in very long compile times and
        /// timeouts.  Small, label-sized documents at 300 to 600 dpi will convert very quickly on most hardware.
        /// </summary>
        /// <param name="format">The destination image format for the conversion</param>
        /// <param name="dpi">Dots per inch in the final converted image</param>
        /// <returns></returns>
        Task SetImageConversion(ImageFormats format, int dpi);

        /// <summary>
        /// Send the command to the server to begin compilation of the session, then asynchronously wait for its results.
        /// </summary>
        /// <returns>The result of the compilation, successful or not</returns>
        Task<ICompileResult> Compile(CancellationToken token = default);
    }

    /// <summary>
    /// The LatexSession class is a handle to a compilation session resource created on the server and provides all
    /// of the methods for interacting with that session.  
    /// </summary>
    internal class LatexSession : ILatexSession
    {
        private readonly FlurlClient _client;
        private readonly string _sessionPath;

        private SessionInfo _info;

        public LatexSession(Uri endpoint, string sessionPath)
        {
            _sessionPath = sessionPath;

            _client = new FlurlClient(endpoint.ToString());
            _client.Configure(s =>
            {
                s.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy(),
                    }

                });
            });
        }

        /// <summary>
        /// Get the full URI of the session resource on the server
        /// </summary>
        public Uri SessionUri => _client.BaseUrl.AppendPathSegment(_sessionPath).ToUri();

        /// <summary>
        /// Set the session to convert the final product to an image format after compilation. Be aware that using
        /// high dpi settings and/or converting large (format) files can result in very long compile times and
        /// timeouts.  Small, label-sized documents at 300 to 600 dpi will convert very quickly on most hardware.
        /// </summary>
        /// <param name="format">The destination image format for the conversion</param>
        /// <param name="dpi">Dots per inch in the final converted image</param>
        /// <returns></returns>
        public async Task SetImageConversion(ImageFormats format, int dpi)
        {
            var response = await _client.Request(_sessionPath)
                .PostJsonAsync(new
                {
                    convert = new
                    {
                        format = format.ToFormatString(),
                        dpi = dpi
                    }
                });
        }

        /// <summary>
        /// Asynchronously get the session information from the server
        /// </summary>
        /// <returns></returns>
        public async Task<SessionInfo> GetInfo()
        {
            _info = await _client.Request(_sessionPath)
                .GetAsync()
                .ReceiveJson<SessionInfo>();
            return _info;
        }

        /// <summary>
        /// Upload a file to the session from a stream. The filename provided is the filename to save the data to
        /// within the session, and can include subdirectory path elements if necessary. The server will not preserve
        /// paths which attempt to leave the session folder.
        /// </summary>
        /// <param name="fileName">The filename and relative path to save the file as within the session</param>
        /// <param name="stream">A stream containing the file data</param>
        /// <returns></returns>
        public async Task UploadFile(string fileName, Stream stream)
        {
            if (_info is null)
                await GetInfo();

            var response = await _client.Request(_info.AddFile.Href)
                .PostMultipartAsync(p => p.AddFile(fileName, stream, fileName));
        }

        /// <summary>
        /// Upload a file to the session from a local file. The filename provided is the filename to save the data to
        /// within the session, and can include sub-directory path elements if necessary. The server will not preserve
        /// paths which attempt to leave the session folder.  The file path provided is the path to the file locally,
        /// which will be opened in read-only mode and its contents sent to the server.
        /// </summary>
        /// <param name="fileName">The filename and relative path to save the file as within the session</param>
        /// <param name="filePath">A path to the file locally which will be opened and saved to the server</param>
        /// <returns></returns>
        public Task UploadFile(string fileName, string filePath)
        {
            return UploadFile(fileName, File.OpenRead(filePath));
        }

        /// <summary>
        /// Upload a template to the session. The target name provided is the file to which the rendered template will
        /// be saved within the session. The template text is the contents of the template file, and the data payload is
        /// an object which will be serialized to JSON and contains the structure for the Jinja2 rendering engine to
        /// insert into the template.
        /// </summary>
        /// <param name="targetName">The name to save the file to after rendering</param>
        /// <param name="templateText">The text contents of the template file</param>
        /// <param name="dataPayload">An object containing the data to be rendered into the template</param>
        /// <returns></returns>
        public async Task UploadTemplate(string targetName, string templateText, object dataPayload)
        {
            if (_info is null)
                await GetInfo();

            var response = await _client.Request(_info.AddTemplates.Href)
                .PostJsonAsync(new
                {
                    target = targetName,
                    text = templateText,
                    data = dataPayload
                });

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error uploading {targetName}: {response.ResponseMessage.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Send the command to the server to begin compilation of the session, then asynchronously wait for its results.
        /// </summary>
        /// <returns>The result of the compilation, successful or not</returns>
        public async Task<ICompileResult> Compile(CancellationToken token = default)
        {
            var response = await _client.Request(SessionUri)
                .PostJsonAsync(new {finalize = true}, cancellationToken: token);

            while (true)
            {
                await GetInfo();
                if (_info.IsFinished)
                {
                    return new CompileResult(_client, _info);
                }

                token.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1.0), token);
            }
        }
    }
}