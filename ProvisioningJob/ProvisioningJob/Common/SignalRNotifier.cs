using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common;
using Common.Model;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ProvisioningJob.Common
{
    public class SignalRNotifier
    {
        private readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(new Guid("2a596627-3c52-4851-b6f1-57301af61e3a").ToByteArray());
        private readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        private static readonly HttpClient Client = new HttpClient();

        private readonly ServiceUtils _serviceUtils;

        private readonly string _hubName;
        private readonly string _serverName;

        private readonly string _endpoint;

        public SignalRNotifier(string connectionString)
        {
            _serverName = GenerateServerName();
            _serviceUtils = new ServiceUtils(connectionString);
            _hubName = Consts.HubName;
            _endpoint = _serviceUtils.Endpoint;
        }

        public void NotifyProgress(ProvisioningState state)
        {
            var url = GetBroadcastUrl();

            var request = BuildRequest(url, new PayloadMessage
            {
                Target = "notify",
                Arguments = new[] { state }
            });

            var response = Task.Run(async () => await Client.SendAsync(request)).Result;

            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpException((int)response.StatusCode, "Exception while sending notification");
            }
        }

        public void NewNotify(ProvisioningState state)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44341/pnpprovisioninghub", opts =>
                {
                    opts.AccessTokenProvider = async () => await GenerateToken();
                })
                .Build();

            Task.Run(async () => await hubConnection.StartAsync()).Wait();
            Task.Run(async () => await hubConnection.SendAsync("Notify", state)).Wait();
        }

        private Task<string> GenerateToken()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _serverName) };
            var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_serverName, "SignalRClients", claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: credentials);
            return Task.FromResult(JwtTokenHandler.WriteToken(token));
        }

        public void NotifyCompleted()
        {
            var url = GetBroadcastUrl();

            var request = BuildRequest(url, new PayloadMessage
            {
                Target = "completed",
                Arguments = new object[] { }
            });

            var response = Task.Run(async () => await Client.SendAsync(request)).Result;

            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                throw new HttpException((int)response.StatusCode, "Exception while sending notification");
            }
        }

        private string GetBroadcastUrl()
        {
            return $"{GetBaseUrl(_hubName)}";
        }

        private string GetBaseUrl(string hubName)
        {
            return $"{_endpoint}/api/v1/hubs/{hubName.ToLower()}";
        }

        private string GenerateServerName()
        {
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private Uri GetUrl(string baseUrl)
        {
            return new UriBuilder(baseUrl).Uri;
        }

        private HttpRequestMessage BuildRequest(string url, PayloadMessage message)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceUtils.GenerateAccessToken(url, _serverName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            return request;
        }
    }
}
