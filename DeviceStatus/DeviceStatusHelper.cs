using log4net;
using PredixCommon;
using PredixCommon.Entities.EdgeManager;
using System;
using System.Linq;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceStack.Redis;
using MimeKit;
using MailKit.Net.Smtp;

namespace DeviceStatus
{
    public class DeviceStatusHelper
    {
        private static ILog logger = LogManager.GetLogger(typeof(DeviceStatusHelper));

        /// <summary>
        /// Get MQTT Client
        /// </summary>
        /// <param name="mqttServerAddress"></param>
        /// <returns></returns>
        public static MqttClient GetMqttClient(string mqttServerAddress, string clientId)
        {
            MqttClient mqttClient;

            LoggerHelper.LogInfoWriter(logger, string.Format("Creating MQTT Client pointing to {0} broker.", mqttServerAddress));

            // create client instance
            mqttClient = new MqttClient(mqttServerAddress);

            LoggerHelper.LogInfoWriter(logger, "Connecting to MQTT Broker...");

            try
            {
                //connect to the broker
                var connectionResult = mqttClient.Connect(clientId, null, null, true, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                    true, DeviceStatusTopics.MQTTStatusTopic, "offline", true, 90);

                LoggerHelper.LogInfoWriter(logger, string.Format("  Connection result: {0}", connectionResult));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogFatalWriter(logger, string.Format("  Error connecting to the MQTT Broker.\n{0}", ex.ToString()));
                return null;
            }

            if (mqttClient.IsConnected)
            {
                LoggerHelper.LogInfoWriter(logger, "  MQTT Client is connected properly. Updating online status.", ConsoleColor.Green);

                //ok just publish you are online properly now!!
                mqttClient.Publish(DeviceStatusTopics.MQTTStatusTopic, Encoding.UTF8.GetBytes("running"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                return mqttClient;
            }
            else
            {
                LoggerHelper.LogFatalWriter(logger, "  MQTT Client not connected to the Broker.");
                return null;
            }
        }


        /// <summary>
        /// Publish from MQTT Device Details
        /// </summary>
        /// <param name="mqttClient"></param>
        /// <param name="dev"></param>
        /// <param name="timeStamp"></param>
        public static void PublishMQTTDeviceDetails(MqttClient mqttClient, DeviceDetails dev, DateTime timeStamp)
        {
            var topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.DeviceName);
            var value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(dev.name, timeStamp).ToJSON());
            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            
            topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.DeviceModel);
            value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(dev.device_model_id, timeStamp).ToJSON());
            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            
            if (dev.deviceInfoStatus.dynamicStatus != null)
            {
                if (dev.deviceInfoStatus.dynamicStatus.networkInfo != null)
                {
                    var tun0Network = dev.deviceInfoStatus.dynamicStatus.networkInfo.Where(ni => ni.name == "tun0").FirstOrDefault();

                    if (tun0Network != null)
                    {
                        topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.IPv6);
                        var IPv6 = tun0Network.ipv6Addresses.FirstOrDefault();
                        value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(IPv6, timeStamp).ToJSON());
                        mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                    }
                }
            }

            if (dev.deviceInfoStatus.simInfo != null)
            {
                var simInfo = dev.deviceInfoStatus.simInfo.FirstOrDefault();

                if (simInfo != null)
                {
                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.iccid);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(simInfo.iccid, timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.imei);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(simInfo.imei, timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    if (simInfo.attributes != null)
                    {
                        if (simInfo.attributes.imsi != null)
                        {
                            topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.imsi);
                            value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(simInfo.attributes.imsi.value, timeStamp).ToJSON());
                            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                        }

                        if (simInfo.attributes.mno != null)
                        {
                            topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.mno);
                            value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(simInfo.attributes.mno.value, timeStamp).ToJSON());
                            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                        }

                        //if (simInfo.attributes.module != null)
                        //{
                        //    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.imsi);
                        //    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(simInfo.attributes.imsi.value, timeStamp).ToJSON());
                        //    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                        //}

                        //if (simInfo.attributes.firmware != null)
                        //{
                        //    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.firmware);
                        //    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(simInfo.attributes.firmware.value, timeStamp).ToJSON());
                        //    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                        //}
                    }
                }
            }

            if (dev.deviceInfoStatus.cellularStatus != null)
            {
                var cellularStatus = dev.deviceInfoStatus.cellularStatus.FirstOrDefault();

                if (cellularStatus != null)
                {
                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.networkMode);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.networkMode, timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rssi);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.signalStrength.rssi.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rsrq);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.signalStrength.rsrq.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rsrp);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.signalStrength.rsrp.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.ecio);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.signalStrength.ecio.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rscp);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.signalStrength.rscp.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.sinr);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp<string>(cellularStatus.signalStrength.sinr.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                }
            }
        }


        /// <summary>
        /// Publish Redis Device Details
        /// </summary>
        /// <param name="redisClient"></param>
        /// <param name="dev"></param>
        /// <param name="timeStamp"></param>
        public static void PublishRedisDeviceDetails(IRedisClient redisClient, DeviceDetails dev, DateTime timeStamp)
        {
            var redisKey = DeviceStatusTopics.RedisDeviceDetails.Replace("{DeviceId}", dev.did);

            var deviceDetailValue = new ValueTimeStamp<DeviceDetails>(dev, timeStamp);

            redisClient.Set<ValueTimeStamp<DeviceDetails>>(redisKey, deviceDetailValue);
        }

        /// <summary>
        /// Get Redis Device Details
        /// </summary>
        /// <param name="redisClient"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static ValueTimeStamp<DeviceDetails> GetRedisDeviceDetails(IRedisClient redisClient, string deviceId)
        {
            var redisKey = DeviceStatusTopics.RedisDeviceDetails.Replace("{DeviceId}", deviceId);

            var deviceDetailValue = redisClient.Get<ValueTimeStamp<DeviceDetails>>(redisKey);
            
            return deviceDetailValue;
        }


        /// <summary>
        /// Publish the list of Devices found in EM
        /// </summary>
        /// <param name="mqttClient"></param>
        /// <param name="deviceCsvList"></param>
        /// <param name="timeStamp"></param>
        public static void PublishMQTTDeviceList(MqttClient mqttClient, List<DeviceDetails> deviceCsvList, DateTime timeStamp)
        {
            var topic = DeviceStatusTopics.MQTTDeviceLisTopic;

            var deviceIdList = (from dev in deviceCsvList
                                 select dev.did).ToList();

            var jsonPayload = new ValueTimeStamp<List<string>>(deviceIdList, timeStamp).ToJSON();
            var value = Encoding.UTF8.GetBytes(jsonPayload);
            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        }

       
        /// <summary>
        /// Publish the list of Devices found in EM
        /// </summary>
        /// <param name="mqttClient"></param>
        /// <param name="deviceCsvList"></param>
        /// <param name="timeStamp"></param>
        public static void PublishRedisDeviceList(IRedisClient redisClient, List<DeviceDetails> deviceCsvList, DateTime timeStamp)
        {
            var redisKey = DeviceStatusTopics.RedisDeviceListKey;

            var deviceIdList = (from dev in deviceCsvList
                               select dev.did).ToList();

            var deviceListValue = new ValueTimeStamp<List<string>>(deviceIdList, timeStamp);

            redisClient.Set<ValueTimeStamp<List<string>>>(redisKey, deviceListValue);
        }

       
        /// <summary>
        /// Connect to the Redis Service
        /// </summary>
        /// <param name="redisHost"></param>
        public static IRedisClient ConnectRedisService(string redisHost)
        {
            LoggerHelper.LogInfoWriter(logger, "Connecting to Redis...");
            var redisManager = new RedisManagerPool(redisHost);

            var redisClient = redisManager.GetClient();
            
            LoggerHelper.LogInfoWriter(logger, "  Connected!", ConsoleColor.Green);

            return redisClient;
        }


        /// <summary>
        /// Get Redis Device List
        /// </summary>
        /// <param name="redisClien"></param>
        /// <returns></returns>
        public static ValueTimeStamp<List<string>> GetRedisDeviceList(IRedisClient redisClient)
        {
            var redisKey = DeviceStatusTopics.RedisDeviceListKey;

            var deviceList = redisClient.Get<ValueTimeStamp<List<string>>>(redisKey);

            return deviceList;
        }








        
        /// <summary>
        /// handler message received!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CheckDeviceDetailsForUpdate(IRedisClient redisClient, ValueTimeStamp<DeviceDetails> valueTimeStamp )
        {
            //check IPv6
            DeviceDetails dev = valueTimeStamp.Value as DeviceDetails; 
            DateTime timeStamp = valueTimeStamp.TimeStamp;

            string deviceID = dev.did;

            //Check for IPv6 change...
            string attribute = DeviceStatusTopics.IPv6;

            if (dev.deviceInfoStatus.dynamicStatus != null)
            {
                if (dev.deviceInfoStatus.dynamicStatus.networkInfo != null)
                {
                    var tun0Network = dev.deviceInfoStatus.dynamicStatus.networkInfo.Where(ni => ni.name == "tun0").FirstOrDefault();

                    if (tun0Network != null)
                    {
                        var IPv6 = tun0Network.ipv6Addresses.FirstOrDefault();

                        //ok now get the value stored in Redis

                        var IPv6ValueFromRedis = GetRedisLastValue<string>(redisClient, deviceID, attribute);

                        if (IPv6ValueFromRedis == null)
                        {
                            //first time?!?!
                            var newValue = new ValueTimeStamp<string>(IPv6, timeStamp);

                            //ok data is fresh! at least update the Redis data
                            UpdateRedisLastValue<string>(redisClient, deviceID, attribute, newValue);

                            //historicize on Redis the status change...
                            UpdateRedisHistory<string>(redisClient, deviceID, attribute, newValue);
                        }
                        else
                        {
                            if (IPv6ValueFromRedis.Value as string != IPv6)
                            {
                                var newValue = new ValueTimeStamp<string>(IPv6, timeStamp);

                                //ok data is fresh! at least update the Redis data
                                UpdateRedisLastValue<string>(redisClient, deviceID, attribute, newValue);

                                string message = string.Format("The {0} is changed for '{1}': at {2} was '{3}' at {4} is '{5}'",
                                    attribute, deviceID, IPv6ValueFromRedis.TimeStamp, IPv6ValueFromRedis.Value,
                                    newValue.TimeStamp, newValue.Value);

                                LoggerHelper.LogInfoWriter(logger, message, ConsoleColor.Yellow);

                                //historicize on Redis the status change...
                                UpdateRedisHistory<string>(redisClient, deviceID, attribute, newValue);
                            }
                        }

                    }
                }
            }


            //check for other attributes change??
        }



        private static ValueTimeStamp<T> GetRedisLastValue<T>(IRedisClient redisClient, string deviceID, string attribute) where T : class
        {
            string redisKeyLastValue = deviceID + "_" + attribute;

            ValueTimeStamp<T> lastValue = null;

            try
            {
                lastValue = redisClient.Get<ValueTimeStamp<T>>(redisKeyLastValue);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogErrorWriter(logger, string.Format("Error deserializing Last value from Redis for {0} {1}\n{2}", deviceID, attribute,
                    ex.ToString()));
            }

            return lastValue;
        }

        public static void UpdateRedisLastValue<T>(IRedisClient redisClient, string deviceID, string attribute, ValueTimeStamp<T> liveDeviceData) where T : class
        {
            string redisKeyLastValue = deviceID + "_" + attribute;

            redisClient.Set<ValueTimeStamp<T>>(redisKeyLastValue, liveDeviceData);
        }
        
        public static void UpdateRedisHistory<T>(IRedisClient redisClient, string deviceID, string attribute, ValueTimeStamp<T> liveDeviceData, int queueLenghHours = 48) where T : class
        {
            //create the redis key
            string redisKeyHistory = deviceID + "_" + attribute + "_History";

            List<ValueTimeStamp<T>> history = null;

            try
            {
                history = redisClient.Get<List<ValueTimeStamp<T>>>(redisKeyHistory);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogErrorWriter(logger, string.Format("Error deserializing History of value from Redis for {0} {1}\n{2}", deviceID, attribute,
                    ex.ToString()));
            }
            
            if (history == null)
            {
                history = new List<ValueTimeStamp<T>>();
            }

            //keep just the last 48 hours! 
            history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(queueLenghHours)).OrderByDescending(i => i.TimeStamp).ToList();

            //add the last update..
            history.Add(liveDeviceData);

            //and save back on Redis..
            redisClient.Set<List<ValueTimeStamp<T>>>(redisKeyHistory, history);
        }
        
        public static List<ValueTimeStamp<T>> GetRedisHistory<T>(IRedisClient redisClient, string deviceID, string attribute) where T : class
        {
            //create the redis key
            string redisKeyHistory = deviceID + "_" + attribute + "_History";
    
            return redisClient.Get<List<ValueTimeStamp<T>>>(redisKeyHistory);
        }


        /// <summary>
        /// Check for the necessity of executing the report...
        /// </summary>
        /// <param name="redisClient"></param>
        public static void CheckHistoryAndSendReport(IRedisClient redisClient, List<string> deviceIdList)
        {
            var now = DateTime.UtcNow;

            //when it was last report??   
            var lastReport = redisClient.Get<DateTime?>("LastSchindlerDeviceReport");

            if (lastReport != null)
            {
                createLastUpdateReportAndSend(redisClient, deviceIdList, lastReport.Value);                
            }

            redisClient.Set<DateTime?>("LastSchindlerDeviceReport", DateTime.UtcNow);
            


            var lastDailyReport = redisClient.Get<DateTime?>("LastSummarySchindlerDeviceReport");

            if ((lastDailyReport != null) && ((now - lastDailyReport.Value) > TimeSpan.FromHours(24)))
            {
                //daily report will include just the devices with multiple IP changes...
                createDailyReportAndSend(redisClient, deviceIdList);

                //fix the report time 10 am UTC  (8AM Swiss time)
                var thisMorning = new DateTime(now.Year, now.Month, now.Day, 06, 0, 0);
                redisClient.Set<DateTime?>("LastSummarySchindlerDeviceReport", thisMorning);
            }
        }

        /// <summary>
        /// createLastUpdateReportAndSend
        /// </summary>
        /// <param name="redisClient"></param>
        /// <param name="deviceIdList"></param>
        /// <param name="value"></param>
        private static void createLastUpdateReportAndSend(IRedisClient redisClient, List<string> deviceIdList, DateTime lastUpdateReportCreation, int historyLenght = 48)
        {
            var message = string.Empty;

            message += "<h1>Schindler Report</h1>";

            var attribute = DeviceStatusTopics.IPv6;

            //currently handling just IPv6!!!
            message += "<br><h2>For the attribute '" + attribute + "' we have the following changes</h2>";

            bool someDevice = false;

            foreach (var deviceId in deviceIdList)
            {
                //get the latest device details...
                var deviceDetails = GetRedisDeviceDetails(redisClient, deviceId);

                if (deviceDetails != null)
                {
                    if (deviceDetails.Value.device_model_id == "VCube")
                    {
                        //skip monitoring for virtual cubes
                        continue;
                    }
                }

                var history = GetRedisHistory<string>(redisClient, deviceId, attribute);

                if (history != null && history.Count > 0)
                {
                    //keep just the last 48 hours! 
                    history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(historyLenght)).OrderByDescending(i => i.TimeStamp).ToList();

                    if (history.Count > 0)
                    {
                        //now check the "latest" ipv6 change... if it is happened after the last report generation get this!
                        var lastTimeStamp = history.First().TimeStamp;

                        if (lastTimeStamp > lastUpdateReportCreation)
                        {
                            message += ("<br><br>Device: " + deviceId + "<br>");

                            someDevice = true;

                            foreach (var item in history)
                            {
                                message += string.Format("<br> {0}", item.ToJSON());
                            }
                        }  
                    }                      
                }
            }

            if (someDevice)
            {
                message += "<br><br><br>";

                SendEmail(new string[] { "Alberto Gorni" }, new string[] { "alberto.gorni@ge.com" }, "[Schindler Last Update Report]", message);

                LoggerHelper.LogInfoWriter(logger, "Sent a last minute device update!", ConsoleColor.DarkYellow);
            }                      
        }

        /// <summary>
        /// create and sent the report of the last 48 hours of changes
        /// </summary>
        private static void createDailyReportAndSend(IRedisClient redisClient, List<string> deviceIdList, int historyLenght = 48)
        {
            LoggerHelper.LogInfoWriter(logger, "Creating and sending daily report...");
            
            var message = string.Empty;

            message += "<h1>Schindler Daily Report</h1>";

            var attribute = DeviceStatusTopics.IPv6;

            message += "<h2>For the attribute '" + attribute + "' we have the following changes</h2>";

            bool someDevice = false;
                
            foreach (var deviceId in deviceIdList)
            {
                //get the latest device details...
                var deviceDetails = GetRedisDeviceDetails(redisClient, deviceId);

                if (deviceDetails != null)
                {
                    if (deviceDetails.Value.device_model_id == "VCube")
                    {
                        //skip monitoring for virtual cubes
                        continue;
                    }
                }

                var history = GetRedisHistory<string>(redisClient, deviceId, attribute);
                                
                if (history != null && history.Count > 0)
                {
                    //keep just the last 48 hours! 
                    history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(historyLenght)).OrderByDescending(i => i.TimeStamp).ToList();

                    //check for IP changes (so... min 2 entries) over the last XX hours...
                    if (history.Count > 1)
                    {
                        message += ("<br><br><b>DeviceId: " + deviceId + "</b><br>");

                        if (deviceDetails != null)
                        {
                            message += ("Device Name: " + deviceDetails.Value.name + "<br>");
                            message += ("Device Model: " + deviceDetails.Value.device_model_id + "<br>");
                            
                            if (deviceDetails.Value.deviceInfoStatus != null)
                            {
                                if ( deviceDetails.Value.deviceInfoStatus.machineInfo != null)
                                {
                                    message += ("Predix Machine Version: " + deviceDetails.Value.deviceInfoStatus.machineInfo.machineVersion + "<br>");
                                }

                                if (deviceDetails.Value.deviceInfoStatus.simInfo != null)
                                {
                                    var firstSimDetails = deviceDetails.Value.deviceInfoStatus.simInfo.FirstOrDefault();

                                    if (firstSimDetails != null)
                                    {
                                        message += ("ICCID: " + firstSimDetails.iccid + "<br>");

                                        if (firstSimDetails.attributes != null)
                                        {
                                            message += ("IMSI: " + firstSimDetails.attributes.imsi.value + "<br>");
                                            message += ("MNO: " + firstSimDetails.attributes.mno.value + "<br>");
                                            message += ("Cellular Module: " + firstSimDetails.attributes.module.value + "<br>");
                                            message += ("Module Firmware: " + firstSimDetails.attributes.firmware.value + "<br>");
                                        }                                        
                                    }
                                }

                                if (deviceDetails.Value.deviceInfoStatus.cellularStatus != null)
                                {
                                    var firstSimDetials = deviceDetails.Value.deviceInfoStatus.cellularStatus.FirstOrDefault();

                                    if (firstSimDetials != null)
                                    {
                                        message += ("Last Mobile Network Mode: " + firstSimDetials.networkMode + "<br>");

                                        if (firstSimDetials.signalStrength != null)
                                        {
                                            message += ("Last RSSI: " + firstSimDetials.signalStrength.rssi + "<br>");
                                            message += ("Last RSRP: " + firstSimDetials.signalStrength.rsrp + "<br>");
                                            message += ("Last RSRQ: " + firstSimDetials.signalStrength.rsrq + "<br>");
                                        }
                                    }
                                }
                            }
                        }



                        message += ("IPv6 Changes: " + history.Count + "<br>");

                        someDevice = true;

                        foreach (var item in history)
                        {
                            message += string.Format("<br> {0}", item.ToJSON());
                        }
                    }
                }
            }
                
            if (!someDevice)
            {
                message += "No changes for the Topic";
            }

            message += "<br><br><br>";
           
            SendEmail(new string[] { "Alberto Gorni"  }, new string[] { "alberto.gorni@ge.com" }, "[Schindler Daily Report]", message);

            LoggerHelper.LogInfoWriter(logger, "  Done!", ConsoleColor.Green);
        }

        public static void SendEmail(string[] sendToName, string[] sendToAddress, string subject, string messageBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Predix Bot", "predixbot@gmail.com"));

            for (int i = 0; i < sendToName.Count(); i++)
            {
                message.To.Add(new MailboxAddress(sendToName[i], sendToAddress[i]));
            }
                       
            message.Subject = subject;

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = messageBody
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("smtp.gmail.com", 465,true);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("predixbot", "Lokiju666");

                client.Send(message);
                client.Disconnect(true);
            }


        }

     
    }
}
