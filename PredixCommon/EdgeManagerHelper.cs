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

        public static System.Net.HttpStatusCode LatestHTTPStatusCode = HttpStatusCode.OK;

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
                await getDeviceListPaginated(edgeManagerBaseUri, accessToken, deviceList, 100, 0);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw;
            }
           
            return deviceList;
        }

        private async static Task getDeviceListPaginated(Uri edgeManagerBaseUri, UAAToken accessToken, DeviceList deviceList, int pageSize, int currentOffset)
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

            LatestHTTPStatusCode = httpResponseMessage.StatusCode;

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
                    await getDeviceListPaginated(edgeManagerBaseUri, accessToken, deviceList, pageSize, currentOffset);
                }
            }
            else
            {  
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }
        }

        public async static Task<DeviceDetails> GetSingleDeviceDetails(string edgeManagerBaseUrl, UAAToken accessToken, string deviceId)
        {
            logger.Debug("Get Single Device Details: " + deviceId);

            HttpClient httpClient = new HttpClient();

            //this is the URL of the UAA
            Uri edgeManagerBaseUri = new Uri(edgeManagerBaseUrl, UriKind.Absolute);

            Uri requestUri = new Uri(edgeManagerBaseUri,
                URIHelper.GetEdgeManagerV1DeviceDetailsRelativeUri(deviceId));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            request.Headers.CacheControl.NoCache = true;
            
            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            LatestHTTPStatusCode = httpResponseMessage.StatusCode;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                var deviceDetails = DeviceDetails.DeserializeStream(new System.IO.StreamReader(contentStream));

                return deviceDetails;
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }

            return null;
        }


        public async static Task AddOrUpdateDeviceModel(string edgeManagerBaseUrl, UAAToken accessToken, DeviceModel deviceModel)
        {
            logger.Debug("Add or Update DeviceModel: " + deviceModel.id);

            HttpClient httpClient = new HttpClient();

            //this is the URL of the UAA
            Uri edgeManagerBaseUri = new Uri(edgeManagerBaseUrl, UriKind.Absolute);

            Uri requestUri = new Uri(edgeManagerBaseUri,
                URIHelper.GetEdgeManagerV1DeviceModelsUriForPUT(deviceModel));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            request.Headers.CacheControl.NoCache = true;

            var deviceModelJSON = Newtonsoft.Json.JsonConvert.SerializeObject(deviceModel);

            request.Content = new StringContent(deviceModelJSON, Encoding.UTF8, "application/json");
            

            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            LatestHTTPStatusCode = httpResponseMessage.StatusCode;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }

            return;
        }





        public async static Task<List<CommandDefinitionResponse>> GetAvailableCommands(string edgeManagerBaseUrl, UAAToken accessToken)
        {
            logger.Debug("Get Available commands.");

            HttpClient httpClient = new HttpClient();

            //this is the URL of the UAA
            Uri edgeManagerBaseUri = new Uri(edgeManagerBaseUrl, UriKind.Absolute);

            Uri requestUri = new Uri(edgeManagerBaseUri, URIHelper.edgeManagerV1GetAvailableCommands);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            request.Headers.CacheControl.NoCache = true;

            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            LatestHTTPStatusCode = httpResponseMessage.StatusCode;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                List<CommandDefinitionResponse> commandList = CommandDefinitionResponse.DeserializeStream(new System.IO.StreamReader(contentStream));

                return commandList;
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }

            return null;
        }



        public async static Task<CommandResponse> ExecuteCommand(string edgeManagerBaseUrl, UAAToken accessToken, CommandRequest commandRequest)
        {
            logger.Debug("Execute Command");

            HttpClient httpClient = new HttpClient();

            //this is the URL of the UAA
            Uri edgeManagerBaseUri = new Uri(edgeManagerBaseUrl, UriKind.Absolute);

            Uri requestUri = new Uri(edgeManagerBaseUri, URIHelper.edgeManagerV1ExecuteCommand);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            request.Headers.CacheControl.NoCache = true;

            var deviceModelJSON = Newtonsoft.Json.JsonConvert.SerializeObject(commandRequest);

            request.Content = new StringContent(deviceModelJSON, Encoding.UTF8, "application/json");


            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            LatestHTTPStatusCode = httpResponseMessage.StatusCode;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                CommandResponse commandResponse = CommandResponse.DeserializeStream(new System.IO.StreamReader(contentStream));

                return commandResponse;
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);

                return null;
            }
        }
        
    }
}
