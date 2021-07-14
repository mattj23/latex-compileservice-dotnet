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
        private SessionInfo _info;

        public LatexSession(Uri sessionUri)
        {
            SessionUri = sessionUri;
            _client = new FlurlClient();
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

        public Uri SessionUri { get; }

        public async Task GetInfo()
        {
            _info = await _client.Request(SessionUri)
                .GetAsync()
                .ReceiveJson<SessionInfo>();
        }

        public async Task UploadFile(string fileName, Stream stream)
        {
            if (_info is null)
                await GetInfo();

            var target = SessionUri.Scheme + "://" + SessionUri.Host.AppendPathSegment(_info.AddFile.Href);

            var response = await _client.Request(target)
                .PostMultipartAsync(p => p.AddFile(fileName, stream, fileName));
        }

        public async Task<CompileResult> Compile()
        {
            var response = await _client.Request(SessionUri)
                .PostJsonAsync(new {finalize = true});

            while (true)
            {
                await GetInfo();
                if (_info.Status == "success")
                {
                    return new CompileResult(_client, _info.Product.Href, true);
                }

                if (_info.Status == "error")
                {
                    return new CompileResult(_client, string.Empty, false);
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0));
            }


        }
    }
}