using log4net;
using PredixCommon.Entities.EdgeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeployDeviceModels
{
    public class DeviceModelCSV
    {
        private static ILog logger = LogManager.GetLogger(typeof(DeviceModelCSV));

        //Device Model Name Description Processor Core #	Memory GB	Storage GB	OS	Location-Code	Location-Description
        public string DeviceModelName { get; set; }

        public string Description { get; set; }

        public string Processor { get; set; }

        public string Core { get; set; }

        public string MemoryGB { get; set; }

        public string StorageGB { get; set; }

        public string OS { get; set; }

        public string Location { get; set; }

        public string DeviceVersion { get; set; }
   

        public DeviceModel ToDeviceModel(IEnumerable<Base64DataCSV> imageCSVList, IEnumerable<Base64DataCSV> iconCSVList)
        {
            DeviceModel deviceModel = new DeviceModel();
            deviceModel.customAttributes = new DeviceModelCustomAttributes();

            deviceModel.coreNum = int.Parse(this.Core);
            (deviceModel.customAttributes as DeviceModelCustomAttributes).Location = this.Location;
            //(deviceModel.customAttributes as DeviceModelCustomAttributes).DeviceType = this.DeviceType.ToString();
            deviceModel.description = this.Description;
            deviceModel.id = this.DeviceModelName;
            deviceModel.memoryGB = double.Parse(this.MemoryGB);
            deviceModel.os = this.OS;
            deviceModel.processor = this.Processor;
            deviceModel.storageGB = double.Parse(this.StorageGB);

            var deviceImageData = imageCSVList.Where(i => i.DeviceVersion == this.DeviceVersion).FirstOrDefault();

            if (deviceImageData != null)
            {
                deviceModel.photo = deviceImageData.Base64Data;
            }
            else
            {
                logger.Error(string.Format("Missing Image data for {0}", this.DeviceVersion));
            }

            var deviceIconData = iconCSVList.Where(i => i.DeviceVersion == this.DeviceVersion).FirstOrDefault();

            if (deviceIconData != null)
            {
                deviceModel.icon = deviceIconData.Base64Data;
            }
            else
            {
                logger.Error(string.Format("Missing Icon data for {0}", this.DeviceVersion));
            }

            return deviceModel;
        }
        
    }

    public class DeviceModelCustomAttributes : ICustomAttributes
    {
        public string Location { get; set; }
        //public string DeviceType { get; set; }
    }

}
