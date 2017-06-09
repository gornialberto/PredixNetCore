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
            var value = Encoding.UTF8.GetBytes(new ValueTimeStamp(dev.name, timeStamp).ToJSON());
            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            
            topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.DeviceModel);
            value = Encoding.UTF8.GetBytes(new ValueTimeStamp(dev.deviceModel, timeStamp).ToJSON());
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
                        value = Encoding.UTF8.GetBytes(new ValueTimeStamp(IPv6, timeStamp).ToJSON());
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
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(simInfo.iccid, timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.imei);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(simInfo.imei, timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    if (simInfo.attributes != null)
                    {
                        if (simInfo.attributes.imsi != null)
                        {
                            topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.imsi);
                            value = Encoding.UTF8.GetBytes(new ValueTimeStamp(simInfo.attributes.imsi.value, timeStamp).ToJSON());
                            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                        }

                        if (simInfo.attributes.mno != null)
                        {
                            topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.mno);
                            value = Encoding.UTF8.GetBytes(new ValueTimeStamp(simInfo.attributes.mno.value, timeStamp).ToJSON());
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
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.networkMode, timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rssi);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.signalStrength.rssi.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rsrq);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.signalStrength.rsrq.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rsrp);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.signalStrength.rsrp.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.ecio);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.signalStrength.ecio.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.rscp);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.signalStrength.rscp.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    topic = DeviceStatusTopics.GetTopic(dev.did, DeviceStatusTopics.sinr);
                    value = Encoding.UTF8.GetBytes(new ValueTimeStamp(cellularStatus.signalStrength.sinr.ToString(), timeStamp).ToJSON());
                    mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                }
            }
        }


        /// <summary>
        /// Publish the list of Devices found in EM
        /// </summary>
        /// <param name="mqttClient"></param>
        /// <param name="deviceCsvList"></param>
        /// <param name="timeStamp"></param>
        public static void PublishMQTTDeviceList(MqttClient mqttClient, List<DeviceDetails> deviceCsvList, DateTime timeStamp)
        {
            var topic = DeviceStatusTopics.MQTTDeviceListTopic;

            var deviceIdList = from dev in deviceCsvList
                                 select dev.did;

            var jsonPayload = new ValueTimeStamp(deviceIdList, timeStamp).ToJSON();
            var value = Encoding.UTF8.GetBytes(jsonPayload);
            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        }










        public static List<string> LatestDeviceIdList = new List<string>();

        public static List<string> CurrentlySubscribedDeviceIdList = new List<string>();

        public static List<string> CurrentlyMonitoredTopicType = new List<string>();


        public static RedisManagerPool RedisManager = null;
        public static IRedisClient RedisClient = null;

        /// <summary>
        /// Connect to the Redis Service
        /// </summary>
        /// <param name="redisHost"></param>
        public static void ConnectRedisService(string redisHost)
        {
            LoggerHelper.LogInfoWriter(logger, "Connecting to Redis...");
            RedisManager = new RedisManagerPool(redisHost);

            RedisClient = RedisManager.GetClient();

            LoggerHelper.LogInfoWriter(logger, "  Connected!", ConsoleColor.Green);
        }


        /// <summary>
        /// Subscribe to the Device List topic
        /// </summary>
        /// <param name="mqttClient"></param>
        public static void SubscribeDeviceStatusTopics(MqttClient mqttClient)
        {
            mqttClient.MqttMsgPublishReceived += MqttClient_DeviceTopicsReceived;
            mqttClient.Subscribe(new string[] { DeviceStatusTopics.MQTTDeviceListTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

            CurrentlyMonitoredTopicType.Add(DeviceStatusTopics.IPv6);
            CurrentlyMonitoredTopicType.Add(DeviceStatusTopics.networkMode);
        }

        /// <summary>
        /// handler message received!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MqttClient_DeviceTopicsReceived(object sender,
            uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            if (e.Topic == DeviceStatusTopics.MQTTDeviceListTopic)
            {
                string messageString = System.Text.Encoding.UTF8.GetString(e.Message);

                var decodedValue = ValueTimeStamp.FromJSON(messageString);

                var deviceListString = decodedValue.Value.ToString();
                
                var deviceList = JsonConvert.DeserializeObject<List<string>>(deviceListString);

                LoggerHelper.LogInfoWriter(logger, string.Format("  Found {0} Device IDs", deviceList.Count));

                LatestDeviceIdList.Clear();
                LatestDeviceIdList.AddRange(deviceList);

                UpdateSubscribedTopic(sender as MqttClient);
            }


            //device model is not considered to be monitored...   we just add in the Redis for pure classification of the data for the Report
            if (CurrentlyMonitoredTopicType.Any(i=> e.Topic.Contains(i)) || (e.Topic.Contains(DeviceStatusTopics.DeviceModel)))
            {
                //ipv6 update!!!  let's see...
                var splittedTopic = e.Topic.Split('/');
                var deviceID = splittedTopic[1];
                var topicType = splittedTopic[2];
                
                //data from Redis
                var currentDeviceData = getRedisLastValue(deviceID, topicType);

                //data live from MQTT
                string messageString = System.Text.Encoding.UTF8.GetString(e.Message);
                var liveDeviceData = ValueTimeStamp.FromJSON(messageString);

                if (liveDeviceData == null)
                {
                    LoggerHelper.LogErrorWriter(logger, string.Format("Impossible to deserialize data for '{0}' - topic {1}",
                        deviceID, topicType));
                }
                else
                {
                    if (currentDeviceData != null)
                    {
                        if (liveDeviceData.TimeStamp > currentDeviceData.TimeStamp)
                        {
                            //ok data is fresh! at least update the Redis data
                            updateRedisLastValue(deviceID, topicType, liveDeviceData);

                            if ((liveDeviceData.Value as string) != (currentDeviceData.Value as string))
                            {
                                //ALLLLLLERRTT!! something is changed!
                                string message = string.Format("The {0} is changed for '{1}': at {2} was '{3}' at {4} is '{5}'",
                                    topicType, deviceID, currentDeviceData.TimeStamp, currentDeviceData.Value,
                                    liveDeviceData.TimeStamp, liveDeviceData.Value);
                            
                                LoggerHelper.LogInfoWriter(logger, message, ConsoleColor.Yellow);

                                //historicize on Redis the status change...
                                updateRedisHistory(deviceID, topicType, liveDeviceData);
                            }
                        }
                    }
                    else
                    {
                        //just update the redis db
                        updateRedisLastValue(deviceID, topicType, liveDeviceData);
                    
                        //and the history as awell
                        updateRedisHistory(deviceID, topicType, liveDeviceData);
                    }
                }
            }       
        }

        private static ValueTimeStamp getRedisLastValue(string deviceID, string topicType)
        {
            string redisKeyLastValue = deviceID + "_" + topicType;

            ValueTimeStamp lastValue = null;

            try
            {
                lastValue = RedisClient.Get<ValueTimeStamp>(redisKeyLastValue);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogErrorWriter(logger, string.Format("Error deserializing Last value from Redis for {0} {1}", deviceID, topicType));
            }

            return lastValue;
        }

        private static void updateRedisLastValue(string deviceID, string topicType, ValueTimeStamp liveDeviceData)
        {
            string redisKeyLastValue = deviceID + "_" + topicType;

            RedisClient.Set<ValueTimeStamp>(redisKeyLastValue, liveDeviceData);
        }

        private static void updateRedisHistory(string deviceID, string topicType, ValueTimeStamp liveDeviceData)
        {
            //create the redis key
            string redisKeyHistory = deviceID + "_" + topicType + "_History";

            List<ValueTimeStamp> history = null;
            try
            {
                history = RedisClient.Get<List<ValueTimeStamp>>(redisKeyHistory);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogErrorWriter(logger, string.Format("Error deserializing History of value from Redis for {0} {1}", deviceID, topicType));
            }
            
            if (history == null)
            {
                history = new List<ValueTimeStamp>();
            }

            //keep just the last 48 hours! 
            history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(48)).ToList();

            //add the last update..
            history.Add(liveDeviceData);

            //and save back on Redis..
            RedisClient.Set<List<ValueTimeStamp>>(redisKeyHistory, history);
        }


        private static List<ValueTimeStamp> getRedisHistory(string deviceID, string topicType)
        {
            //create the redis key
            string redisKeyHistory = deviceID + "_" + topicType + "_History";
    
            return RedisClient.Get<List<ValueTimeStamp>>(redisKeyHistory);
        }


        public static void CheckHistoryAndSendReport()
        {
            //when it was last report??
            var lastReport = RedisClient.Get<DateTime?>("LastSchindlerDeviceReport");

            if (lastReport != null)
            {
                if (lastReport < DateTime.UtcNow - TimeSpan.FromMinutes(10.0))
                {
                    createAndSendReport();
                }
            }
            else
            {
                createAndSendReport();
            }
        }

        /// <summary>
        /// create and sent the report of the last 48 hours of changes
        /// </summary>
        private static void createAndSendReport()
        {
            LoggerHelper.LogInfoWriter(logger, "Creating and sending report...");
            
            var message = string.Empty;

            message += "Schindler Report\n\n";

            foreach (var topicType in CurrentlyMonitoredTopicType)
            {
                message += @"\n\nFor the Topic '" + topicType + "' we have the following changes:\n";

                bool someDevice = false;

                var deviceAndModelList = from dev in CurrentlySubscribedDeviceIdList
                                    select new { DeviceID = dev, DeviceModel = getRedisLastValue(dev, DeviceStatusTopics.DeviceModel) };

                var deviceByModelGroups = deviceAndModelList.GroupBy(g => g.DeviceModel);

                foreach (var deviceModelGroup in deviceByModelGroups)
                {
                    var deviceModel = deviceModelGroup.Key;

                    if (deviceModel != null && !string.IsNullOrEmpty(deviceModel.Value as string))
                    {
                        message += ("\nDeviceModel: " + (deviceModel.Value as string) + "\n");
                    }
                    
                    foreach (var deviceAndModel in deviceModelGroup)
                        {
                            var deviceID = deviceAndModel.DeviceID;

                            var history = getRedisHistory(deviceID,topicType);

                            if (history != null && history.Count > 0)
                            {
                                message += ("\n\n\nDevice: " + deviceID + "\n");

                                someDevice = true;
                        
                                //keep just the last 48 hours! 
                                history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(48)).ToList();

                                foreach (var item in history)
                                {
                                    message += string.Format("\n {0}", item.ToJSON());
                                }
                            }
                    }
                }

                if (!someDevice)
                {
                    message += "No changes for the Topic";
                }

                message += "\n\n\n";
            }

            RedisClient.Set<DateTime?>("LastSchindlerDeviceReport", DateTime.UtcNow);

            SendEmail(message);

            LoggerHelper.LogInfoWriter(logger, "  Done!", ConsoleColor.Green);
        }

        public static void SendEmail(string messageBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Predix Bot", "predixbot@gmail.com"));
            message.To.Add(new MailboxAddress("Gorni Alberto", "gorni.alberto@gmail.com"));
            //message.To.Add(new MailboxAddress("Nicolandrea Costa", "nicolandrea.costa@ge.com"));
            message.Subject = "[Schindler Notification]";

            message.Body = new TextPart("plain")
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

        private static void UpdateSubscribedTopic(MqttClient mqttClient)
        {
            //now update the subscription if needed!

            //check for newely added devices
            var addedDevices = from dev in LatestDeviceIdList
                               where !CurrentlySubscribedDeviceIdList.Any(d => d == dev)
                               select dev;

            var removedDevices = from dev in CurrentlySubscribedDeviceIdList
                                 where !LatestDeviceIdList.Any(d => d == dev)
                                 select dev;

            //ok now subscribe / unsubscribe...

            if (addedDevices.Count() > 0)
            {
                LoggerHelper.LogInfoWriter(logger, string.Format("  Found {0} NEW Device IDs", addedDevices.Count()), ConsoleColor.Green);


                var topicToSubscribe = (from dev in addedDevices
                                        select DeviceStatusTopics.GetTopic(dev, DeviceStatusTopics.IPv6)).ToArray();

                var qosToSubscribe = (from dev in addedDevices
                                      select MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE).ToArray();

                mqttClient.Subscribe(topicToSubscribe, qosToSubscribe); 
            }

            if (removedDevices.Count() > 0)
            {
                LoggerHelper.LogInfoWriter(logger, string.Format("  Found {0} REMOVED Device IDs", removedDevices.Count()), ConsoleColor.Red);


                var topicToUnSubscribe = from dev in removedDevices
                                         select DeviceStatusTopics.GetTopic(dev, DeviceStatusTopics.IPv6);

                mqttClient.Unsubscribe(topicToUnSubscribe.ToArray());
            }

            CurrentlySubscribedDeviceIdList.Clear();
            CurrentlySubscribedDeviceIdList.AddRange(LatestDeviceIdList);
        }
    }
}
