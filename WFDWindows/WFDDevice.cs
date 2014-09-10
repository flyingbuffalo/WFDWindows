using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Devices.Enumeration;

namespace Buffalo.WiFiDirect
{
    public class WFDDevice
    {
        bool bDevice = false;
        private DeviceInformation wfdDevInfo = null;

        public WFDDevice(DeviceInformation wfdDevInfo)
        {
            this.wfdDevInfo = wfdDevInfo;
            bDevice = true;
        }

        public Object WFDDeviceInfo
        {
            get
            {
                return wfdDevInfo;
            }
        }

        public bool IsDevice
        {
            get
            {
                return bDevice;
            }
        }
    }
}
