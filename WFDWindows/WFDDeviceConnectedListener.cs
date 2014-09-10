using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace Buffalo.WiFiDirect
{
    public interface WFDDeviceConnectedListener
    {
        public void onDeviceConnected(StreamSocket s);
        public void onDeviceDisconnected();
    }
}
