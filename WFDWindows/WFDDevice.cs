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
        private bool bDevice = false;
        private DeviceInformation wfdDevInfo = null;

        public WFDDevice(DeviceInformation wfdDevInfo)
        {
            this.wfdDevInfo = wfdDevInfo;
            bDevice = true;
        }

        internal Object WFDDeviceInfo
        {
            get
            {
                return wfdDevInfo;
            }
        }

        internal bool IsDevice
        {
            get
            {
                return bDevice;
            }
        }

        public string Name {
            get { return wfdDevInfo.Name; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
