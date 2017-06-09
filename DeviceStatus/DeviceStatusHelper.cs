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
            history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(queueLenghHours)).ToList();

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
            //when it was last report??
            var lastReport = redisClient.Get<DateTime?>("LastSchindlerDeviceReport");

            if (lastReport != null)
            {
                if (lastReport < DateTime.UtcNow - TimeSpan.FromMinutes(1.0))
                {
                    createAndSendReport(redisClient, deviceIdList);
                }
            }
            else
            {
                createAndSendReport(redisClient, deviceIdList);
            }
        }

        /// <summary>
        /// create and sent the report of the last 48 hours of changes
        /// </summary>
        private static void createAndSendReport(IRedisClient redisClient, List<string> deviceIdList)
        {
            LoggerHelper.LogInfoWriter(logger, "Creating and sending report...");
            
            var message = string.Empty;

            message += "Schindler Report\n\n";

            var attribute = DeviceStatusTopics.IPv6;

            message += "\n\nFor the attribute '" + attribute + "' we have the following changes:\n";

            bool someDevice = false;
                
            foreach (var deviceId in deviceIdList)
            {
                var history = GetRedisHistory<string>(redisClient, deviceId, attribute);

                if (history != null && history.Count > 0)
                {
                    message += ("\n\n\nDevice: " + deviceId + "\n");

                    someDevice = true;
                        
                    //keep just the last 48 hours! 
                    history = history.Where(i => i.TimeStamp > DateTime.UtcNow - TimeSpan.FromHours(48)).ToList();

                    foreach (var item in history)
                    {
                        message += string.Format("\n {0}", item.ToJSON());
                    }
                }
            }
                

            if (!someDevice)
            {
                message += "No changes for the Topic";
            }

            message += "\n\n\n";
            

            redisClient.Set<DateTime?>("LastSchindlerDeviceReport", DateTime.UtcNow);

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

     
    }
}
