using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace Buffalo.WiFiDirect
{
    public class WFDPairInfo
    {
        //private 
        private DependencyObject parentUI;
        private WFDDevice device;
        private EndpointPair deviceEndpointPair = null;
        private StreamSocket socket;

        internal WFDPairInfo(WFDDevice device, EndpointPair deviceEndpointPair, DependencyObject parent)
        {
            this.device = device;
            this.deviceEndpointPair = deviceEndpointPair;
            this.parentUI = parent;
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
            Debug.WriteLine("connectSocketAsync");

            StreamSocketListener socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += async (StreamSocketListener sender,
                    StreamSocketListenerConnectionReceivedEventArgs args) =>
                {
                    Debug.WriteLine("ConnectionReceived");
                    //windows-device connection, conncted callback
                    StreamSocket s = args.Socket;
                    
                    await parentUI.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Debug.WriteLine("Call onSocketConnected");
                        
                        l.onSocketConnected(s);
                    });
                    
                    /*Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                    (Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        l.onSocketConnected(s);
                    });*/
                };

            parentUI.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    await socketListener.BindServiceNameAsync("9190");
                });
        }
        
        public interface PairSocketConnectedListener
        {
		    void onSocketConnected(StreamSocket s);
        }

    }
}
