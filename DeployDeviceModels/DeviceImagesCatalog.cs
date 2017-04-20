using System;
using System.Collections.Generic;
using System.Text;

namespace DeployDeviceModels
{
    public static class DeviceImagesCatalog 
    {
        static DeviceImagesCatalog()
        {
            CatalogByDeviceType = new Dictionary<DeviceTypeEnum, DeviceImages>();

        }

        public static Dictionary<DeviceTypeEnum, DeviceImages> CatalogByDeviceType { get; set; }
    }
}
