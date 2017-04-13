using log4net;
using log4net.Repository.Hierarchy;
using PredixCommon.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PredixCommon
{
    public class UAAHelper
    {
        private static ILog logger = LogManager.GetLogger(typeof(UAAHelper));

        /// <summary>
        /// GetClientCredentialsGrantAccessTokenBySvcOpsClient
        /// 
        /// Gets an Access Token using as grant type "client_credentials" and using as credentials teh svcops client id and client secret
        /// </summary>
        /// <param name="tenantInformation"></param>
        /// <returns></returns>
        public async static Task<UAAToken> GetClientCredentialsGrantAccessToken(string baseUAAUrl, string clientID, string clientSecret)
        {
            logger.Debug("GetClientCredentialsGrantAccessTokenBySvcOpsClient from '" + baseUAAUrl);

            UAAToken tokenResponse = null;

            //this is the URL of the UAA
            Uri baseUAAUri = new Uri(baseUAAUrl, UriKind.Absolute);

            var httpClient = new HttpClient();

            //HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = baseUAAUri;

            List<KeyValuePair<string, string>> nameValueCollection = new List<KeyValuePair<string, string>>();
            nameValueCollection.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

            System.Net.Http.FormUrlEncodedContent formContent = new FormUrlEncodedContent(nameValueCollection);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, URIHelper.uaaTokenRequestRelativeUri);

            request.Content = formContent;

            string clientId_clientSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientID + ":" + clientSecret));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", clientId_clientSecret);

            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                tokenResponse = new UAAToken(new System.IO.StreamReader(contentStream));
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }

            return tokenResponse;
        }
    }
}
