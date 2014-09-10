using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buffalo.WiFiDirect
{
    public interface WFDDeviceDiscoveredListener
    {
        public void onDevicesDiscovered(List<WFDDevice> deviceList);
    }
}
