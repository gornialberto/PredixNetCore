using log4net;
using log4net.Repository.Hierarchy;
using PredixCommon.Entities;
using PredixCommon.Entities.EdgeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PredixCommon
{
    public class EdgeManagerHelper
    {
        private static ILog logger = LogManager.GetLogger(typeof(EdgeManagerHelper));

        /// <summary>
        /// GetClientCredentialsGrantAccessTokenBySvcOpsClient
        /// 
        /// Gets an Access Token using as grant type "client_credentials" and using as credentials teh svcops client id and client secret
        /// </summary>
        /// <param name="tenantInformation"></param>
        /// <returns></returns>
        public async static Task<DeviceList> GetDeviceList(string edgeManagerBaseUrl, UAAToken accessToken)
        {
            DeviceList deviceList = new DeviceList();

            logger.Debug("GetDeviceList");

            //this is the URL of the UAA
            Uri edgeManagerBaseUri = new Uri(edgeManagerBaseUrl, UriKind.Absolute);
            
            try
            {
                await paginatedCall(edgeManagerBaseUri, accessToken, deviceList, 100, 0);
            }
            catch (Exception ex)
            {
                throw;
            }
           
            return deviceList;
        }


        private async static Task paginatedCall(Uri edgeManagerBaseUri, UAAToken accessToken, DeviceList deviceList, int pageSize, int currentOffset)
        {
            HttpClient httpClient = new HttpClient();
            
            List<KeyValuePair<string, object>> queryParameters = new List<KeyValuePair<string, object>>();
            queryParameters.Add(new KeyValuePair<string, object>("limit", pageSize));
            queryParameters.Add(new KeyValuePair<string, object>("offset", currentOffset));

            Uri requestUri = new Uri(edgeManagerBaseUri, 
                URIHelper.GetEdgeManagerV1DeviceListRelativeUriWithQueryParameters(queryParameters));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
           
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            request.Headers.CacheControl.NoCache = true;

            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                var tempDeviceList = DeviceList.DeserializeStream(new System.IO.StreamReader(contentStream));

                deviceList.Devices.AddRange(tempDeviceList);

                var totalCount = httpResponseMessage.Headers.GetValues("Total-Count").FirstOrDefault();

                int totalCountInt = int.Parse(totalCount);

                if (deviceList.Devices.Count() < totalCountInt)
                {
                    currentOffset += pageSize;
                    await paginatedCall(edgeManagerBaseUri, accessToken, deviceList, pageSize, currentOffset);
                }
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }
        }
    }
}
