﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using PredixCommon;
using PredixCommon.Entities.EdgeManager;

namespace PredixCommon
{
    public static class URIHelper
    {
        /// <summary>
        /// UAA Token Request path
        /// </summary>
        public static Uri uaaTokenRequestRelativeUri = new Uri("/oauth/token", UriKind.Relative);

        //https://em-device-apidocs.run.aws-usw02-dev.ice.predix.io/swagger-ui.html
        //https://em-app-apidocs.run.aws-usw02-dev.ice.predix.io/swagger-ui.html
        private const string edgeManagerV1DeviceListRelativeUrl = "/svc/device/v1/device-mgmt/devices";

        public static Uri edgeManagerV1DeviceListRelativeUri = new Uri(edgeManagerV1DeviceListRelativeUrl, UriKind.Relative);


        public static Uri GetEdgeManagerV1DeviceListRelativeUriWithQueryParameters(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var uri = new Uri(edgeManagerV1DeviceListRelativeUrl + parameters.AsQueryString(), UriKind.Relative);

            return uri;
        }

        public static Uri GetEdgeManagerV1DeviceDetailsRelativeUri(string deviceId)
        {
            var uri = new Uri(edgeManagerV1DeviceListRelativeUrl + "/" + deviceId, UriKind.Relative);

            return uri;
        }


        //return new Uri(string.Format(uriTemplate, tenantInformation.UAAId), UriKind.Absolute);

        public const string edgeManagerV1DeviceModelsRelativeUrl = "/svc/device/v1/device-mgmt/device_models";

        public static Uri GetEdgeManagerV1DeviceModelsUriForPUT(DeviceModel deviceModel)
        {
            var uri = new Uri(edgeManagerV1DeviceModelsRelativeUrl + "/" + deviceModel.id, UriKind.Relative);

            return uri;
        }

        /// <summary>
        /// Add query string parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string AsQueryString(this IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (!parameters.Any())
                return "";

            var builder = new StringBuilder("?");

            var separator = "";
            foreach (var kvp in parameters.Where(kvp => kvp.Value != null))
            {
                builder.AppendFormat("{0}{1}={2}", separator, WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value.ToString()));

                separator = "&";
            }

            return builder.ToString();
        }


        public static Uri edgeManagerV1GetAvailableCommands = new Uri("/svc/command/v1/commands", UriKind.Relative);

        public static Uri edgeManagerV1ExecuteCommand = new Uri("svc/command/v1/commands/devices", UriKind.Relative);

        public static string edgeManagerV1GetCommandTaskOutputTemplate = "/svc/command/v1/tasks/{{taskId}}/output";

        public static Uri GetEdgeManagerV1GetCommandTaskOutputUri(string taskId)
        {
            return new Uri(edgeManagerV1GetCommandTaskOutputTemplate.Replace("{{taskId}}", taskId), UriKind.Relative);
        }
            
        ///
        ///TIME SERIES AREA
        ///

        public static Uri timeSeriesV1InjestionRelativeUri = new Uri("/v1/stream/messages", UriKind.Relative);

        public static Uri timeSeriesQueryTagRelativeUri = new Uri("/v1/tags", UriKind.Relative);

        public static Uri timeSeriesQueryTagValueRelativeUri = new Uri("/v1/datapoints", UriKind.Relative);
        
    }
}
