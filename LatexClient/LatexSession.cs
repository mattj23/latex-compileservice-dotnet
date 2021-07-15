using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LatexClient
{
    public class LatexSession
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

        public Uri SessionUri => _client.BaseUrl.AppendPathSegment(_sessionPath).ToUri();

        public async Task GetInfo()
        {
            _info = await _client.Request(_sessionPath)
                .GetAsync()
                .ReceiveJson<SessionInfo>();
        }

        public async Task UploadFile(string fileName, Stream stream)
        {
            if (_info is null)
                await GetInfo();

            var response = await _client.Request(_info.AddFile.Href)
                .PostMultipartAsync(p => p.AddFile(fileName, stream, fileName));
        }

        public async Task<CompileResult> Compile()
        {
            var response = await _client.Request(SessionUri)
                .PostJsonAsync(new {finalize = true});

            while (true)
            {
                await GetInfo();
                if (_info.IsFinished)
                {
                    return new CompileResult(_client, _info);
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0));
            }


        }
    }
}