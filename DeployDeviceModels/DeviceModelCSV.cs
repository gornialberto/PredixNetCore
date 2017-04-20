using PredixCommon.Entities.EdgeManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeployDeviceModels
{
    public class DeviceModelCSV
    {
        //Device Model Name Description Processor Core #	Memory GB	Storage GB	OS	Location-Code	Location-Description
        public string DeviceModelName { get; set; }

        public string Description { get; set; }

        public string Processor { get; set; }

        public string Core { get; set; }

        public string MemoryGB { get; set; }

        public string StorageGB { get; set; }

        public string OS { get; set; }

        public string Location { get; set; }

        public DeviceTypeEnum DeviceType { get; set; }
   

        public DeviceModel ToDeviceModel()
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

            deviceModel.photo = DeviceImagesCatalog.CatalogByDeviceType[this.DeviceType].PhotoBase64;
            deviceModel.icon = DeviceImagesCatalog.CatalogByDeviceType[this.DeviceType].IconBase64;

            return deviceModel;
        }
    }

    public class DeviceModelCustomAttributes : ICustomAttributes
    {
        public string Location { get; set; }
        //public string DeviceType { get; set; }
    }

}
