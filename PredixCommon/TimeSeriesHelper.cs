using log4net;
using Newtonsoft.Json;
using PredixCommon.Entities;
using PredixCommon.Entities.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PredixCommon
{
    public class TimeSeriesHelper
    {
        private static ILog logger = LogManager.GetLogger(typeof(TimeSeriesHelper));

        /// <summary>
        /// Retreive the list of all the tags actually configured into a specific Time Seriez Zone ID
        /// </summary>
        /// <param name="baseTimeSeriesQueryUri"></param>
        /// <param name="timeSeriesZoneId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async static Task<List> GetFullTagListOfTimeSeriesZoneId(string baseTimeSeriesQueryUrl, string timeSeriesZoneId, UAAToken accessToken)
        {
            logger.Debug("Get Full Tag List from Time Series Zone ID: " + timeSeriesZoneId);

            List tagListResponse = null;

            Uri baseTimeSeriesQueryUri = new Uri(baseTimeSeriesQueryUrl, UriKind.Absolute);

            Uri tagQueryUri = new Uri(baseTimeSeriesQueryUri, URIHelper.timeSeriesQueryTagRelativeUri);

            HttpClient httpClient = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, tagQueryUri);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.Add("Predix-Zone-Id", timeSeriesZoneId);

            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                try
                {
                    tagListResponse = new List(new System.IO.StreamReader(contentStream));
                }
                catch (Exception ex)
                {
                    logger.Fatal(string.Format("Fatal error deserializing the result of Tag Query:\n{0}", ex.ToString()));
                    throw;
                }
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }

            tagListResponse.Tags = tagListResponse.Tags.OrderBy(e => e).ToArray(); //order tags by name!

            return tagListResponse;
        }




        /// <summary>
        /// Query Time Series
        /// </summary>
        /// <typeparam name="TimeSeriesQueryResponse"></typeparam>
        /// <param name="baseTimeSeriesQueryUrl"></param>
        /// <param name="timeSeriesZoneId"></param>
        /// <param name="accessToken"></param>
        /// <param name="tagList"></param>
        /// <returns></returns>
        public async static Task<TimeSeriesQueryResponse> QueryTimeSeries<TimeSeriesQueryResponse>(string baseTimeSeriesQueryUrl, 
            string timeSeriesZoneId, UAAToken accessToken, ITimeSeriesQuery query) 
            where TimeSeriesQueryResponse : class, ITimeSeriesQueryResponse
        {
            logger.Debug("QueryTimeSeries data from Time Series Zone ID: " + timeSeriesZoneId);

            TimeSeriesQueryResponse response = null;

            Uri baseTimeSeriesQueryUri = new Uri(baseTimeSeriesQueryUrl, UriKind.Absolute);

            Uri tagDataPointQueryUri = new Uri(baseTimeSeriesQueryUri, URIHelper.timeSeriesQueryTagValueRelativeUri);

            HttpClient httpClient = new HttpClient();

            var jsonQuery = query.GetJsonQuery();

            logger.Debug(string.Format("JSON QUERY: \n{0}", jsonQuery));

            System.Net.Http.StringContent jsonContent = new StringContent(jsonQuery);
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tagDataPointQueryUri);
            request.Content = jsonContent;

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(accessToken.TokenType, accessToken.AccessToken);
            request.Headers.Add("Predix-Zone-Id", timeSeriesZoneId);

            logger.Debug("Sending Http Request");

            var httpResponseMessage = await httpClient.SendAsync(request);

            logger.Debug("Http Request executed");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.Debug("Http Response Success Status Code " + httpResponseMessage.StatusCode);

                var readString = await httpResponseMessage.Content.ReadAsStringAsync();

                response = JsonConvert.DeserializeObject<TimeSeriesQueryResponse>(readString);                                
            }
            else
            {
                logger.Error("Http Response Failure Status Code " + httpResponseMessage.StatusCode);
            }

            return response;
        }




        /// <summary>
        /// Get the WebSocket Client connected to the Ingestion Url
        /// </summary>
        /// <param name="timeSeriesWSSUrl"></param>
        /// <returns></returns>
        public async static Task<System.Net.WebSockets.ClientWebSocket> GetWebSocketConnection(string timeSeriesWSSBaseUrl, string timeSeriesZoneId, UAAToken accessToken)
        {
            System.Net.WebSockets.ClientWebSocket cli = new ClientWebSocket();

            cli.Options.SetRequestHeader("Authorization", "Bearer " + accessToken.AccessToken);
            cli.Options.SetRequestHeader("Predix-Zone-Id", timeSeriesZoneId);
            cli.Options.SetRequestHeader("Origin", "https://localhost");
            cli.Options.SetRequestHeader("Content-Type", "application/json");

            //cli.Options.Proxy = new GEProxy();

            Uri timeSeriesWSSBaseUri = new Uri(timeSeriesWSSBaseUrl, UriKind.Absolute);

            Uri timeSeriesWSSUri = new Uri(timeSeriesWSSBaseUri, URIHelper.timeSeriesV1InjestionRelativeUri);


            await cli.ConnectAsync(timeSeriesWSSUri, CancellationToken.None);

            return cli;
        }

        static UTF8Encoding encoder = new UTF8Encoding();

        /// <summary>
        /// Injest data in Time Series...
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async static Task IngestData(ClientWebSocket webSocket, IEnumerable<DataPoints> data)
        {
            //ok now create the JSON Payload 

            var jsonPayloadObj = new PredixCommon.Entities.TimeSeries.InjestionJSON(data);

            var jsonPayload = JsonConvert.SerializeObject(jsonPayloadObj,Formatting.Indented);

            byte[] buffer = encoder.GetBytes(jsonPayload);
            
            //send and ignore the result.... 
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
