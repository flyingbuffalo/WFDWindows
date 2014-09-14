using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace Buffalo.WiFiDirect
{
    public class WFDPairInfo
    {
        private WFDDevice device;
        private EndpointPair deviceEndpointPair = null;
        private StreamSocket socket;

        internal WFDPairInfo(WFDDevice device, EndpointPair deviceEndpointPair)
        {
            this.device = device;
            this.deviceEndpointPair = deviceEndpointPair;
        }

        public HostName getLocalAddress()
        {
            return deviceEndpointPair.LocalHostName;
        }

        public HostName getRemoteAddress()
        {
            return deviceEndpointPair.RemoteHostName;
        }

        internal WFDDevice getWFDDevice()
        {
            return device;
        }


        /*public StreamSocket getSocket()
        {
            StreamSocketListener socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += onConnection;
        }*/


        public void connectSocketAsync(PairSocketConnectedListener l)
        {
            StreamSocketListener socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += (StreamSocketListener sender,
                    StreamSocketListenerConnectionReceivedEventArgs args) =>
                {
                    //windows-device connection, conncted callback
                    StreamSocket s = args.Socket;
                    Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                    (Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        l.onSocketConnected(s);
                    });
                };
        }
        
        public interface PairSocketConnectedListener
        {
		    void onSocketConnected(StreamSocket s);
        }

    }
}
