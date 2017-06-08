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

namespace DeviceStatusMQTT
{
    public class DeviceStatusMQTTHelper
    {
        private static ILog logger = LogManager.GetLogger(typeof(DeviceStatusMQTTHelper));

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

        public static void PublishMQTTDeviceList(MqttClient mqttClient, List<DeviceDetails> deviceCsvList, DateTime timeStamp)
        {
            var topic = DeviceStatusTopics.MQTTDeviceListTopic;

            var deviceIdList = from dev in deviceCsvList
                                 select dev.did;

            var jsonPayload = new ValueTimeStamp(deviceIdList, timeStamp).ToJSON();
            var value = Encoding.UTF8.GetBytes(jsonPayload);
            mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        }


        public static List<string> SubscribedDeviceIDs = new List<string>();

        /// <summary>
        /// Subscribe to the Device List topic
        /// </summary>
        /// <param name="mqttClient"></param>
        public static void SubscribeDeviceStatusTopics(MqttClient mqttClient)
        {
            mqttClient.MqttMsgPublishReceived += MqttClient_DeviceTopicsReceived;
            mqttClient.Subscribe(new string[] { DeviceStatusTopics.MQTTDeviceListTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

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

                //now update the subscription if needed!

                ////check for newely added devices
                //var addedDevices = from dev in deviceList
                //                   where !SubscribedDeviceIDs.Any(d => d == dev)
                //                   select dev;

                //LoggerHelper.LogInfoWriter(logger, string.Format("  Found {0} NEW Device IDs", addedDevices.Count()), ConsoleColor.Green);

                //var removedDevices = from dev in SubscribedDeviceIDs
                //                     where !deviceList.Any(d => d == dev)
                //                     select dev;

                //LoggerHelper.LogInfoWriter(logger, string.Format("  Found {0} REMOVED Device IDs", removedDevices.Count()), ConsoleColor.Red);

                ////ok now subscribe / unsubscribe...

                //if (addedDevices.Count() > 0)
                //{
                //    var topicToSubscribe = (from dev in addedDevices
                //                           select DeviceStatusTopics.GetTopic(dev, DeviceStatusTopics.IPv6)).ToArray();

                //    var qosToSubscribe = (from dev in removedDevices
                //                         select MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE).ToArray();

                //    (sender as MqttClient).Subscribe(topicToSubscribe, qosToSubscribe );
                //}

                //if(removedDevices.Count() > 0)
                //{
                //    var topicToUnSubscribe = from dev in removedDevices
                //                             select DeviceStatusTopics.GetTopic(dev, DeviceStatusTopics.IPv6);

                //    (sender as MqttClient).Unsubscribe(topicToUnSubscribe.ToArray());
                //}

                SubscribedDeviceIDs.Clear();
                SubscribedDeviceIDs.AddRange(deviceList);
            }

            if (e.Topic.Contains(DeviceStatusTopics.IPv6))
            {
                //ipv6 update!!!  let's see...
                var splittedTopic = e.Topic.Split('/');
                var deviceID = splittedTopic[1];

            }
            
        }
    }
}
