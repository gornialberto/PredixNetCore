using log4net;
using PredixCommon.Entities;
using PredixEntities;
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
        public async static Task<PredixTimeSeriesTagList> GetFullTagListOfTimeSeriesZoneId(string baseTimeSeriesQueryUrl, string timeSeriesZoneId, UAAToken accessToken)
        {
            logger.Debug("Get Full Tag List from Time Series Zone ID: " + timeSeriesZoneId);

            PredixTimeSeriesTagList tagListResponse = null;

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
                    tagListResponse = new PredixTimeSeriesTagList(new System.IO.StreamReader(contentStream));
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

        public async static Task<PredixTimeSeriesQueryResponse> GetLastSamples(string baseTimeSeriesQueryUrl, string timeSeriesZoneId, UAAToken accessToken, 
            PredixTimeSeriesTagList tagList)
        {
            logger.Debug("GetLastSamples from Time Series Zone ID: " + timeSeriesZoneId);

            PredixTimeSeriesQueryResponse response = null;

            Uri baseTimeSeriesQueryUri = new Uri(baseTimeSeriesQueryUrl, UriKind.Absolute);

            Uri tagDataPointQueryUri = new Uri(baseTimeSeriesQueryUri, URIHelper.timeSeriesQueryTagValueRelativeUri);

            HttpClient httpClient = new HttpClient();
            
            string listOfTag = string.Empty;

            foreach (var tagName in tagList.Tags)
            {
                listOfTag += "\"" + tagName + "\",";
            }

            if (listOfTag.Length > 0)
            {
                listOfTag = listOfTag.TrimEnd(',');
            }

            string jsonRequest =
 @"{
      ""start"":""60d-ago"",
	  ""tags"":[{
		    		""name"":[**LIST_OF_TAG**],
			     	""limit"":1,
				    ""order"": ""desc"",
				    ""filters"": {
                	                ""qualities"": {
                    	                              ""values"": [""3""]
                                                   }
                                 }
			}]	
}".Replace("**LIST_OF_TAG**", listOfTag);

            System.Net.Http.StringContent jsonContent = new StringContent(jsonRequest);


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

                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                response = PredixTimeSeriesQueryResponse.Create(new System.IO.StreamReader(contentStream));
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

            cli.Options.SetRequestHeader("Predix-Zone-Id", timeSeriesZoneId);
            cli.Options.SetRequestHeader("Origin", "localhost");
            cli.Options.SetRequestHeader("Authorization", "bearer " + accessToken.AccessToken);


            Uri timeSeriesWSSBaseUri = new Uri(timeSeriesWSSBaseUrl, UriKind.Absolute);

            Uri timeSeriesWSSUri = new Uri(timeSeriesWSSBaseUri, URIHelper.timeSeriesV1InjestionRelativeUri);
     
            await cli.ConnectAsync(timeSeriesWSSUri, CancellationToken.None);

            return cli;
        }
    }
}
